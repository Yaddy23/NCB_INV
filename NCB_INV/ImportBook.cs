using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDataReader;
using System.Windows.Forms;

namespace NCB_INV
{
    public partial class ImportBook : Form
    {
        public ImportBook()
        {
            InitializeComponent();
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
                MessageBox.Show("Error loading inventory: " + ex.Message);
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
            using (var editor = new BookEditorForm(null)) // ADD MODE
            {
                if (editor.ShowDialog() == DialogResult.OK)
                {
                    // LF ISBN exist
                    if (DBConnection.DoesISBNExist(editor.BookData.ISBN))
                    {
                        MessageBox.Show("Error: A book with this ISBN already exists!",
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
                // 1. Convert the selected row back into a Book object
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

                // 2. Open the form in 'Modify' mode by passing the object
                using (var editor = new BookEditorForm(selected))
                {
                    if (editor.ShowDialog() == DialogResult.OK)
                    {
                        // 3. Save to DB and Refresh
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
                    using (var stream = System.IO.File.Open(ofd.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
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
                                row[0].ToString(),                                      // ISBN
                                row[1]?.ToString() ?? "",                               // Title
                                row[2]?.ToString() ?? "",                               // Edition
                                row[3]?.ToString() ?? "",                               // Year
                                row[4]?.ToString() ?? "",                               // Author
                                row[5]?.ToString() ?? "",                               // Bind
                                row[6] == DBNull.Value ? 0 : Convert.ToInt32(row[6]),   // Qty
                                row[7] == DBNull.Value ? 0m : Convert.ToDecimal(row[7]), // Price
                                row[8]?.ToString() ?? ""                                // Publisher
                                 );

                                DBConnection.SaveBook(excelBook);

                            }
                        }
                    }
                    MessageBox.Show("Import successful!");
                    RefreshBookList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error importing data: " + ex.Message);
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
                    $"Are you sure you want to delete '{title}'?",
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
                        MessageBox.Show("Error deleting from database: " + ex.Message);
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
