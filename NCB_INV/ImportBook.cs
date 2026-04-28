using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;
using ExcelDataReader;
using static NCB_INV.DBConnection;

namespace NCB_INV
{
    public partial class ImportBook : Form
    {
        private System.Windows.Forms.Timer syncTimer;
        private System.Windows.Forms.Timer conTimer;
        public ImportBook()
        {
            InitializeComponent();
            SetupAutoSync();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            conTimer = new System.Windows.Forms.Timer();
            conTimer.Interval = 5000;
            conTimer.Tick += ConTimer_Tick;
            conTimer.Start();

        }

        private async void ConTimer_Tick(object? sender, EventArgs e)
        {
            bool isConnected = await Task.Run(() => IsInternetAvailable());
            UpdateUI(isConnected);
        }

        private bool IsInternetAvailable()
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = ping.Send("8.8.8.8", 2000);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch { return false; }
        }

        private void UpdateUI(bool isConnected)
        {
            if (isConnected)
            {
                lblNetStatus.Text = "● Online";
                lblNetStatus.ForeColor = Color.Green;
            }
            else
            {
                lblNetStatus.Text = "○ Offline (Local Only)";
                lblNetStatus.ForeColor = Color.Red;
            }
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
                    lblSyncStatus.Text = "Status: Offline (Local Only)";
                    lblSyncStatus.ForeColor = Color.OrangeRed;
                }
                return;
            }

            isSyncing = true;
            lblSyncStatus.Text = "Status: Syncing & Resolving...";
            lblSyncStatus.ForeColor = Color.Blue;

            try
            {
                await Task.Run(async () => await DBConnection.ExecuteDeltaSync());

               this.Invoke((MethodInvoker)delegate
                {
                    RefreshBookList();
                    lblSyncStatus.Text = $"Last Sync: {DateTime.Now:hh:mm:ss tt}";
                    lblSyncStatus.ForeColor = Color.Green;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Sync Failure: {ex.Message}");
                this.Invoke((MethodInvoker)delegate {
                    lblSyncStatus.Text = "Status: Sync Failed";
                    lblSyncStatus.ForeColor = Color.Red;
                });
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

        private void ApplyPermissions()
        {
            bool isAdmin = CurrentSession.User.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            btnImport.Visible = isAdmin;
            btnDeleteBook.Visible = isAdmin;
            btnModifyBook.Visible = true;
            btnScanBook.Visible = true;
            btnAddBook.Visible = true;
        }

        private async void ImportBook_Load(object sender, EventArgs e)
        {
            Color primaryNavy = ColorTranslator.FromHtml("#2C3E50");
            Color accentBlue = ColorTranslator.FromHtml("#3498DB");
            Color bgLight = ColorTranslator.FromHtml("#F4F7F6");

            this.BackColor = bgLight;
            dgvBookList.EnableHeadersVisualStyles = false;
            dgvBookList.ColumnHeadersDefaultCellStyle.BackColor = primaryNavy;
            dgvBookList.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            ApplyPermissions();
            RefreshBookList();

            await RunBackgroundSync();
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
            using var editor = new BookEditorForm(null);

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

        private void btnModifyBook_Click(object sender, EventArgs e)
        {
            if (dgvBookList.SelectedRows.Count > 0)
            {
                var row = dgvBookList.SelectedRows[0];
                int oldQty = Convert.ToInt32(row.Cells["Qty"].Value);

                Book selected = new(
                    row.Cells["ISBN"].Value.ToString(),
                    row.Cells["Title"].Value.ToString(),
                    row.Cells["Edition"].Value.ToString(),
                    row.Cells["Year"].Value.ToString(),
                    row.Cells["Author"].Value.ToString(),
                    row.Cells["Bind"].Value.ToString(),
                    oldQty,
                    Convert.ToDecimal(row.Cells["Price"].Value),
                    row.Cells["Publisher"].Value.ToString(),
                    DateTime.Now
                );

                using var editor = new BookEditorForm(selected);

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
        private void btnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new() { Filter = "Excel Files|*.xlsx;*.xls" };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;

                    var batchList = new List<Book>();
                    var transactionBatch = new List<Transaction>();
                    int totalImportedCount = 0;

                    using var stream = File.Open(ofd.FileName, FileMode.Open, FileAccess.Read);
                    using var reader = ExcelReaderFactory.CreateReader(stream);

                    var result = reader.AsDataSet();
                    var table = result.Tables[0];

                    for (int i = 1; i < table.Rows.Count; i++)
                    {
                        var row = table.Rows[i];
                        if (row[0] == DBNull.Value || string.IsNullOrWhiteSpace(row[0].ToString()))
                            continue;

                        Book excelBook = new(
                            row[0].ToString().Trim(),
                            row[1]?.ToString() ?? "",
                            row[2]?.ToString() ?? "",
                            row[3]?.ToString() ?? "",
                            row[4]?.ToString() ?? "",
                            row[5]?.ToString() ?? "",
                            row[6] == DBNull.Value ? 0 : Convert.ToInt32(row[6]),
                            row[7] == DBNull.Value ? 0m : Convert.ToDecimal(row[7]),
                            row[8]?.ToString() ?? "",
                            DateTime.Now
                        );

                        batchList.Add(excelBook);

                        transactionBatch.Add(new Transaction
                        {
                            ISBN = excelBook.ISBN,
                            Title = excelBook.Title,
                            ChangeAmount = excelBook.Qty,
                            NewTotal = excelBook.Qty.ToString(),
                            Reason = "Excel Bulk Import",
                            performedBy = CurrentSession.User.DisplayName,
                            Timestamp = DateTime.Now
                        });

                        totalImportedCount++;

                        if (batchList.Count >= 5000)
                        {
                            DBConnection.BulkImportBooks(batchList);
                            DBConnection.LogBulkTransactions(transactionBatch);

                            batchList.Clear();
                            transactionBatch.Clear();
                        }
                    }

                    if (batchList.Count > 0)
                    {
                        DBConnection.BulkImportBooks(batchList);
                        DBConnection.LogBulkTransactions(transactionBatch);
                    }

                    DBConnection.LogTransaction(
                        "BULK_IMPORT_SUMMARY",
                        $"File: {Path.GetFileName(ofd.FileName)} ({totalImportedCount} items)",
                        totalImportedCount,
                        "N/A",
                        "Bulk Import Execution",
                        CurrentSession.User.DisplayName
                    );

                    MessageBox.Show($"Import Successful! {totalImportedCount} books and logs processed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshBookList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error during import: " + ex.Message, "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            BookScanner bookScanner = new();
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

        private void ImportBook_FormClosing(object sender, FormClosingEventArgs e)
        {
            CompactLocalDatabase();
        }
    }
}