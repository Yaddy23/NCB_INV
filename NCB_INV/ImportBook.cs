using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
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
            syncTimer.Interval = 300000;
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
                this.Invoke((MethodInvoker)delegate
                {
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

            int total = freshData.AsEnumerable().Sum(r => r.Field<int?>("Qty") ?? 0);
            lblTotalStocks.Text = $"Total Stocks: {total}";

            //Re - bind the data source
            dgvBookList.DataSource = freshData;
        }

        private void ApplyPermissions()
        {
            bool isAdmin = CurrentSession.User.Role.Contains("Admin", StringComparison.OrdinalIgnoreCase);


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
                    DBConnection.SaveBook(editor.BookData, editor.BookData.AuthorId, editor.BookData.PublisherId);

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

                int oldQty = row.Cells["Qty"].Value != DBNull.Value ? Convert.ToInt32(row.Cells["Qty"].Value) : 0;
                decimal price = row.Cells["Price"].Value != DBNull.Value ? Convert.ToDecimal(row.Cells["Price"].Value) : 0m;

                Book selected = new Book(
                    row.Cells["Subject"].Value?.ToString() ?? "",
                    row.Cells["ISBN"].Value?.ToString() ?? "",
                    row.Cells["Title"].Value?.ToString() ?? "",
                    row.Cells["Edition"].Value?.ToString() ?? "",
                    row.Cells["Year"].Value?.ToString() ?? "",
                    row.Cells["Author"].Value?.ToString() ?? "Unknown",
                    row.Cells["Bind"].Value?.ToString() ?? "",
                    oldQty,
                    price,
                    row.Cells["Publisher"].Value?.ToString() ?? "Unknown",
                    DateTime.Now
                );

                using var editor = new BookEditorForm(selected);
                if (editor.ShowDialog() == DialogResult.OK)
                {
                    DBConnection.SaveBook(editor.BookData, editor.BookData.AuthorId, editor.BookData.PublisherId);

                    int change = editor.BookData.Qty - oldQty;
                    DBConnection.LogTransaction(
                        editor.BookData.ISBN,
                        editor.BookData.Title,
                        change,
                        editor.BookData.Qty.ToString(),
                        "Stock Modified",
                        DBConnection.CurrentSession.User?.DisplayName ?? "Unknown"
                    );

                    RefreshBookList();
                }
            }
        }
        private async void btnImport_Click(object sender, EventArgs e)
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

                    // Ensure you have "using System.IO;" at the top
                    using var stream = File.Open(ofd.FileName, FileMode.Open, FileAccess.Read);
                    using var reader = ExcelReaderFactory.CreateReader(stream);

                    var result = reader.AsDataSet();
                    var table = result.Tables[0];

                    for (int i = 1; i < table.Rows.Count; i++)
                    {
                        var row = table.Rows[i];
                        if (row[1] == DBNull.Value || string.IsNullOrWhiteSpace(row[1].ToString()))
                            continue;

                        string rawisbn = row[1].ToString().Trim();
                        string cleanIsbn = rawisbn.Replace("-", "").Replace(" ", "");

                        Book excelBook = new Book(
                            row[0]?.ToString() ?? "",
                            cleanIsbn,
                            row[2]?.ToString() ?? "",
                            row[3]?.ToString() ?? "",
                            row[4]?.ToString() ?? "",
                            row[5]?.ToString() ?? "Unknown",
                            row[6]?.ToString() ?? "",
                            row[7] == DBNull.Value ? 0 : Convert.ToInt32(row[7]),
                            row[8] == DBNull.Value ? 0m : Convert.ToDecimal(row[8]),
                            row[9]?.ToString() ?? "Unknown",
                            DateTime.Now
                        )
                        ;

                        batchList.Add(excelBook);
                        totalImportedCount++;

                        // Process in batches of 5000 for speed
                        if (batchList.Count >= 5000)
                        {
                            await Task.Run(() => DBConnection.BulkImportBooks(batchList));
                            batchList.Clear();
                        }
                    }

                    // Final batch
                    if (batchList.Count > 0)
                    {
                        await Task.Run(() => DBConnection.BulkImportBooks(batchList));
                    }

                    // Sync the local SQLite database so the Scanner sees the new books
                    await DBConnection.ExecuteDeltaSync();

                    MessageBox.Show($"Import Successful! {totalImportedCount} books processed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshBookList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
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

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string query = txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(query))
            {
                var allBooks = DBConnection.GetLocalBooks();
                dgvBookList.DataSource = allBooks;

                lblSuggestion.Visible = false;
                return;
            }

            var allLocalBooks = DBConnection.GetLocalBooks();
            var (results, suggestion) = SearchEngine.FuzzySearch(query, allLocalBooks);

            if (results.Any())
            {
                dgvBookList.DataSource = results;
                lblSuggestion.Visible = false;
            }
            else if (!string.IsNullOrEmpty(suggestion))
            {
                string cleanSuggestion = suggestion.Replace("\r", "").Replace("\n", " ");

                lblSuggestion.Text = $"Did you mean: {cleanSuggestion}?";
                lblSuggestion.Visible = true;
                lblSuggestion.MaximumSize = new Size(0, 0);
                dgvBookList.DataSource = null;
            }
            else
            {
                lblSuggestion.Visible = false;
                dgvBookList.DataSource = null;
            }
        }

        private void lblSuggestion_Click(object sender, EventArgs e)
        {
            string suggestedWord = lblSuggestion.Text
            .Replace("Did you mean: ", "")
            .Replace("?", "")
            .Trim();

            txtSearch.Text = suggestedWord;
        }
    }
}