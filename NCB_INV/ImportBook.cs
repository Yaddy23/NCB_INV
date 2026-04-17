using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ExcelDataReader;
using System.IO;

namespace NCB_INV
{
    public partial class ImportBook : Form
    {
        public ImportBook()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void RefreshBookList()
        {
            try
            {
                DataTable dt = DBConnection.GetInventory();
                dgvBookList.DataSource = dt;
                dgvBookList.AutoResizeColumns();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cloud Connection Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                Book selected = new Book(
                    row.Cells["ISBN"].Value.ToString(),
                    row.Cells["Title"].Value.ToString(),
                    row.Cells["Edition"].Value.ToString(),
                    row.Cells["Year"].Value.ToString(),
                    row.Cells["Author"].Value.ToString(),
                    row.Cells["Bind"].Value.ToString(),
                    Convert.ToInt32(row.Cells["Qty"].Value),
                    Convert.ToDecimal(row.Cells["Price"].Value),
                    row.Cells["Publisher"].Value.ToString()
                );

                using (var editor = new BookEditorForm(selected))
                {
                    if (editor.ShowDialog() == DialogResult.OK)
                    {
                        DBConnection.SaveBook(editor.BookData);
                        RefreshBookList();
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a book to edit.");
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

                    MessageBox.Show("Cloud Bulk Import Successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                string isbn = dgvBookList.SelectedRows[0].Cells["ISBN"].Value.ToString();
                string title = dgvBookList.SelectedRows[0].Cells["Title"].Value.ToString();

                DialogResult result = MessageBox.Show(
                    $"Are you sure you want to delete '{title}' from the Cloud Database?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        DBConnection.DeleteBook(isbn);
                        RefreshBookList();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error deleting from Cloud: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a book from the table first.");
            }
        }

        private void btnScanBook_Click(object sender, EventArgs e)
        {
            BookScanner bookScanner = new BookScanner();
            bookScanner.Show();
        }
    }
}