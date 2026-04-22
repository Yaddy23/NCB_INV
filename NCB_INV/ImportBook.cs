using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ExcelDataReader;
using static NCB_INV.DBConnection;

namespace NCB_INV
{
    public partial class ImportBook : Form
    {
        private System.Windows.Forms.Timer syncTimer;
        public ImportBook()
        {
            InitializeComponent();
            SetupAutoSync();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void SetupAutoSync()
        {
            syncTimer = new System.Windows.Forms.Timer();
            syncTimer.Interval = 30000;
            syncTimer.Tick += async (s, e) => await RunBackgroundSync();
            syncTimer.Start();
        }

        private bool isSyncing = false;
        private async Task RunBackgroundSync()
        {
            if (isSyncing || !DBConnection.IsCloudAvailable())
            {
                if (!DBConnection.IsCloudAvailable())
                {
                    lblSyncStatus.Text = "Status: Cloud Offline (Saving locally)";
                    lblSyncStatus.ForeColor = Color.OrangeRed;
                }
                return;
            }

            isSyncing = true;
            lblSyncStatus.Text = "Status: Syncing...";
            lblSyncStatus.ForeColor = Color.Blue;

            try
            {
                await Task.Run(() => DBConnection.SyncOfflineData());

                this.Invoke((MethodInvoker)delegate
                {
                    RefreshBookList();
                });

                lblSyncStatus.Text = $"Last Sync: {DateTime.Now:hh:mm:ss tt}";
                lblSyncStatus.ForeColor = Color.Green;
            }
            catch
            {
                lblSyncStatus.Text = "Status: Sync Failed (Retrying)";
                lblSyncStatus.ForeColor = Color.Red;
            }
            finally
            {
                isSyncing = false;
            }
        }

        private void RefreshBookList()
        {
            DataTable freshData = DBConnection.GetInventory();

            // Re-bind the data source
            dgvBookList.DataSource = freshData;
        }

        private void ImportBook_Load(object sender, EventArgs e)
        {
            RefreshBookList();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            dgvBookList.DataSource = DBConnection.SearchBooks(txtSearch.Text);
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            RefreshBookList();
        }

        private void btnAddBook_Click(object sender, EventArgs e)
        {
            using (var editor = new BookEditorForm(null))
            {
                if (editor.ShowDialog() == DialogResult.OK)
                {
                    if (DBConnection.DoesISBNExist(editor.BookData.ISBN))
                    {
                        MessageBox.Show("Error: A book with this ISBN already exists in the cloud!",
                                        "Duplicate Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        DBConnection.SaveBook(editor.BookData);

                        DBConnection.LogTransaction(
                        editor.BookData.ISBN,
                        editor.BookData.Title,
                        editor.BookData.Qty,    
                        editor.BookData.Qty.ToString(),      
                        "New Book Added",          
                        CurrentSession.User.DisplayName 
                );
                        RefreshBookList();
                    }
                }
            }
        }

        private void btnModifyBook_Click(object sender, EventArgs e)
        {
            if (dgvBookList.SelectedRows.Count > 0)
            {
                var row = dgvBookList.SelectedRows[0];
                int oldQty = Convert.ToInt32(row.Cells["Qty"].Value);

                Book selected = new Book(
                    row.Cells["ISBN"].Value.ToString(),
                    row.Cells["Title"].Value.ToString(),
                    row.Cells["Edition"].Value.ToString(),
                    row.Cells["Year"].Value.ToString(),
                    row.Cells["Author"].Value.ToString(),
                    row.Cells["Bind"].Value.ToString(),
                    oldQty,
                    Convert.ToDecimal(row.Cells["Price"].Value),
                    row.Cells["Publisher"].Value.ToString()
                );

                using (var editor = new BookEditorForm(selected))
                {
                    if (editor.ShowDialog() == DialogResult.OK)
                    {
                        DBConnection.SaveBook(editor.BookData);

                        int change = editor.BookData.Qty - oldQty;
                        DBConnection.LogTransaction(
                            editor.BookData.ISBN,
                            editor.BookData.Title,
                            change,                
                            editor.BookData.Qty.ToString(),     
                            "Stock Modified",
                            CurrentSession.User.DisplayName
                        );

                        RefreshBookList();
                    }
                }
            }
        }
        private void btnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Excel Files|*.xlsx;*.xls" };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    List<Book> batchList = new List<Book>();
                    int totalImportedCount = 0;

                    using (var stream = File.Open(ofd.FileName, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var result = reader.AsDataSet();
                            var table = result.Tables[0];

                            for (int i = 1; i < table.Rows.Count; i++)
                            {
                                var row = table.Rows[i];
                                if (row[0] == DBNull.Value || string.IsNullOrWhiteSpace(row[0].ToString()))
                                    continue;

                                Book excelBook = new Book(
                                    row[0].ToString().Trim(),
                                    row[1]?.ToString() ?? "",
                                    row[2]?.ToString() ?? "",
                                    row[3]?.ToString() ?? "",
                                    row[4]?.ToString() ?? "",
                                    row[5]?.ToString() ?? "",
                                    row[6] == DBNull.Value ? 0 : Convert.ToInt32(row[6]),
                                    row[7] == DBNull.Value ? 0m : Convert.ToDecimal(row[7]),
                                    row[8]?.ToString() ?? ""
                                );

                                batchList.Add(excelBook);
                                totalImportedCount++;

                                if (batchList.Count >= 5000)
                                {
                                    DBConnection.BulkImportBooks(batchList);
                                    batchList.Clear();
                                }
                            }
                        }
                    }

                    if (batchList.Count > 0)
                    {
                        DBConnection.BulkImportBooks(batchList);
                    }

                    DBConnection.LogTransaction(
                        "BULK_IMPORT",
                        $"File: {Path.GetFileName(ofd.FileName)} ({totalImportedCount} books)",
                        totalImportedCount,
                        "0",             
                        "Bulk Import",
                        CurrentSession.User.DisplayName
                    );

                    MessageBox.Show($"Cloud Bulk Import Successful! Total: {totalImportedCount} books.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshBookList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error importing to Cloud: " + ex.Message, "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void btnDeleteBook_Click(object sender, EventArgs e)
        {
            if (dgvBookList.SelectedRows.Count > 0)
            {
                var row = dgvBookList.SelectedRows[0];
                string isbn = row.Cells["ISBN"].Value.ToString();
                string title = row.Cells["Title"].Value.ToString();
                int lastQty = Convert.ToInt32(row.Cells["Qty"].Value);

                if (MessageBox.Show($"Delete '{title}'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    DBConnection.DeleteBook(isbn);

                    DBConnection.LogTransaction(
                        isbn,
                        title,
                        -lastQty,
                        "0",       
                        "Book Deleted",
                        CurrentSession.User.DisplayName
                    );

                    RefreshBookList();
                }
            }
        }

        private void btnScanBook_Click(object sender, EventArgs e)
        {
            BookScanner bookScanner = new BookScanner();
            bookScanner.Show();
        }

        private void dgvBookList_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (this.dgvBookList.Columns[e.ColumnIndex].Name == "Qty" && e.Value != null)
            {
                if (int.TryParse(e.Value.ToString(), out int qty))
                {
                    DataGridViewRow row = dgvBookList.Rows[e.RowIndex];
                    if (qty == 0)
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 200, 200); // Light Red
                    else if (qty <= 5)
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 200); // Light Yellow
                    else
                        row.DefaultCellStyle.BackColor = Color.White; // Reset if high stock
                }
            }
        }
    }
}