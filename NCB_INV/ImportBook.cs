using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
        private System.Windows.Forms.Timer? syncTimer;
        private System.Windows.Forms.Timer conTimer;
        private readonly System.Windows.Forms.Timer _searchDebouncer = new() { Interval = 300 };
        public ImportBook()
        {
            InitializeComponent();
            SetupAutoSync();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            conTimer = new System.Windows.Forms.Timer
            {
                Interval = 5000
            };
            conTimer.Tick += ConTimer_Tick;
            conTimer.Start();
            _searchDebouncer.Tick += Debouncer_Tick;
        }

        private async void Debouncer_Tick(object? sender, EventArgs e)
        {
            _searchDebouncer.Stop();
            string query = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(query))
            {
                await RefreshBookList();
                lblSuggestion.Text = "";
                return;
            }

            DataTable BooksInventory = await DBConnection.GetInventoryAsync();

            List<Book> allbooks = ConvertDataTableToList(BooksInventory);

            var (results, suggestion) = await Task.Run(() => SearchEngine.FuzzySearch(query, allbooks));

            if (results != null && results.Count > 0)
            {
                dgvBookList.DataSource = DBConnection.ToDataTable(results);
                lblSuggestion.Visible = false;
            }
            else if (!string.IsNullOrEmpty(suggestion))
            {
                lblSuggestion.Text = $"Did you mean: {suggestion.Replace("\r", "").Replace("\n", " ")}?";
                lblSuggestion.Visible = true;
                dgvBookList.DataSource = null;
            }
            else
            {
                dgvBookList.DataSource = null;
            }

        }

        private void EnableDoubleBuffering(DataGridView dgv)
        {
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.SetProperty,
                null, dgv, new object[] { true });
        }
        private async void ConTimer_Tick(object? sender, EventArgs e)
        {
            bool isConnected = await Task.Run(() => IsInternetAvailable());
            UpdateUI(isConnected);
        }

        private static bool IsInternetAvailable()
        {
            try
            {
                using Ping ping = new();
                PingReply reply = ping.Send("8.8.8.8", 2000);
                return reply.Status == IPStatus.Success;
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

            syncTimer = new System.Windows.Forms.Timer
            {
                Interval = 300000
            };
            syncTimer.Tick += async (s, e) => await RunBackgroundSync();
            syncTimer.Start();
        }

        private bool isSyncing = false;
        private async Task RunBackgroundSync()
        {
            bool isCloudUp = await Task.Run(() => DBConnection.IsCloudAvailable());

            if (isSyncing || !isCloudUp)
            {
                if (!isCloudUp)
                {
                    lblSyncStatus.Text = isCloudUp ? "Status: Sync in Progress..." : "Status: Cloud Unreachable";
                    lblSyncStatus.ForeColor = isCloudUp ? Color.Orange : Color.Red;
                }
                return;
            }

            isSyncing = true;
            lblSyncStatus.Text = "Status: Syncing & Resolving...";
            lblSyncStatus.ForeColor = Color.Blue;

            try
            {
                await Task.Run(async () => await DBConnection.ExecuteDeltaSync());

                await RefreshBookList();
                lblSyncStatus.Text = $"Last Sync: {DateTime.Now:hh:mm:ss tt}";
                lblSyncStatus.ForeColor = Color.Green;

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

        private async Task RefreshBookList()
        {
            this.Cursor = Cursors.WaitCursor;

            DataTable freshData = await Task.Run(() => DBConnection.GetInventory());


            int total = freshData.AsEnumerable().Sum(r => r.Field<int?>("Qty") ?? 0);
            lblTotalStocks.Text = $"Total Stocks: {total}";

            dgvBookList.SuspendLayout();

            var previousSizeMode = dgvBookList.AutoSizeColumnsMode;
            dgvBookList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvBookList.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            //Re - bind the data source
            dgvBookList.DataSource = freshData;

            if (dgvBookList.Columns.Contains("LastModified"))
            {
                dgvBookList.Columns["LastModified"].Visible = false;
                dgvBookList.Columns["Title"].FillWeight = 300;
                dgvBookList.Columns["ISBN"].FillWeight = 120;
                dgvBookList.Columns["Qty"].FillWeight = 50;
            }

            dgvBookList.AutoSizeColumnsMode = previousSizeMode;
            dgvBookList.ResumeLayout();

            this.Cursor = Cursors.Default;
        }

        private void ApplyPermissions()
        {
            bool isAdmin = CurrentSession.User!.Role.Contains("Admin", StringComparison.OrdinalIgnoreCase);


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
            dgvBookList.VirtualMode = true;
            dgvBookList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            EnableDoubleBuffering(dgvBookList);

            BackupLocalDatabase();
            ApplyPermissions();
            await RefreshBookList();
            await RunBackgroundSync();


        }

        private async void BtnReload_Click(object sender, EventArgs e)
        {
            await RefreshBookList();
        }

        private async void BtnAddBook_Click(object sender, EventArgs e)
        {
            using var editor = new BookEditorForm(null);

            if (editor.ShowDialog() == DialogResult.OK)
            {
                bool exists = await Task.Run(() => DBConnection.DoesISBNExist(editor.BookData!.ISBN));
                if (exists)
                {
                    this.Cursor = Cursors.Default;
                    MessageBox.Show("Error: A book with this ISBN already exists in the cloud!",
                                    "Duplicate Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    await Task.Run(() =>
                    {
                        DBConnection.SaveBook(editor.BookData!, editor.BookData!.AuthorId, editor.BookData!.PublisherId);
                        DBConnection.LogTransaction(
                            editor.BookData!.ISBN, editor.BookData!.Title, editor.BookData!.Qty,
                            editor.BookData!.Qty.ToString(), "New Book Added", CurrentSession.User!.DisplayName);
                    });

                    await RefreshBookList();
                }
            }
        }

        private async void BtnModifyBook_Click(object sender, EventArgs e)
        {
            if (dgvBookList.SelectedRows.Count > 0)
            {
                var row = dgvBookList.SelectedRows[0];

                int oldQty = row.Cells["Qty"].Value != DBNull.Value ? Convert.ToInt32(row.Cells["Qty"].Value) : 0;
                decimal price = row.Cells["Price"].Value != DBNull.Value ? Convert.ToDecimal(row.Cells["Price"].Value) : 0m;

                Book selected = new(
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
                    if (editor.BookData != null && DBConnection.CurrentSession.User != null)
                    {
                        await Task.Run(() =>
                        {
                            int change = editor.BookData.Qty - oldQty;
                            DBConnection.SaveBook(editor.BookData, editor.BookData.AuthorId, editor.BookData.PublisherId);
                            DBConnection.LogTransaction(
                                editor.BookData.ISBN, editor.BookData.Title, editor.BookData.Qty,
                                editor.BookData.Qty.ToString(), "New Book Added", DBConnection.CurrentSession.User.DisplayName);
                        });

                        await RefreshBookList();
                    }
                }
            }
        }
        private async void BtnImport_Click(object sender, EventArgs e)
        {
            var resultCheck = MessageBox.Show("Do you have the formatted Excel template for import?",
            "Template Check", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (resultCheck == DialogResult.Cancel) return;

            if (resultCheck == DialogResult.No)
            {
                GenerateExcelTemplate();
                return;
            }

            OpenFileDialog ofd = new() { Filter = "Excel Files|*.xlsx;*.xls" };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    var batchList = new List<Book>();

                    var uniqueAuthors = new HashSet<string>();
                    var uniquePublishers = new HashSet<string>();

                    int totalImportedCount = 0;

                    using var stream = File.Open(ofd.FileName, FileMode.Open, FileAccess.Read);
                    using var reader = ExcelReaderFactory.CreateReader(stream);

                    var result = reader.AsDataSet();
                    var table = result.Tables[0];

                    for (int i = 1; i < table.Rows.Count; i++)
                    {
                        var row = table.Rows[i];
                        if (row[1] == DBNull.Value || string.IsNullOrWhiteSpace(row[1].ToString()))
                            continue;

                        string rawisbn = row[1].ToString()!.Trim();
                        string cleanIsbn = rawisbn.Replace("-", "").Replace(" ", "");

                        int qty = 0;
                        if (row[7] != DBNull.Value)
                        {
                            string rawqty = row[7].ToString()!.Trim();
                            if (double.TryParse(rawqty, out double parsedQty))
                            {
                                qty = (int)Math.Round(parsedQty);
                            }
                        }

                        string authorName = row[5]?.ToString()?.Trim() ?? "Unknown";
                        string pubName = row[9]?.ToString()?.Trim() ?? "Unknown";

                        uniqueAuthors.Add(authorName);
                        uniquePublishers.Add(pubName);

                        Book excelBook = new(
                            row[0]?.ToString() ?? "",
                            cleanIsbn,
                            row[2]?.ToString() ?? "",
                            row[3]?.ToString() ?? "",
                            row[4]?.ToString() ?? "",
                            authorName,
                            row[6]?.ToString() ?? "",
                            qty,
                            row[8] == DBNull.Value ? 0m : Convert.ToDecimal(row[8]),
                            pubName,
                            DateTime.Now
                        );

                        batchList.Add(excelBook);
                        totalImportedCount++;

                        if (batchList.Count >= 5000)
                        {
                            await Task.Run(() => DBConnection.BulkImportBooks(batchList, [.. uniqueAuthors], [.. uniquePublishers]));

                            batchList.Clear();
                            uniqueAuthors.Clear();
                            uniquePublishers.Clear();
                        }
                    }

                    if (batchList.Count > 0)
                    {
                        await Task.Run(() => DBConnection.BulkImportBooks(batchList, [.. uniqueAuthors], [.. uniquePublishers]));
                    }

                    await DBConnection.ExecuteDeltaSync();
                    MessageBox.Show($"Import Successful! {totalImportedCount} books processed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await RefreshBookList();
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

        // Helper method to generate the template
        private static void GenerateExcelTemplate()
        {
            SaveFileDialog sfd = new()
            {
                Filter = "Excel Workbook|*.xlsx;*.xls",
                FileName = "Book_Import_Template.xlsx"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var workbook = new ClosedXML.Excel.XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Books");

                        string[] headers = ["Subject", "ISBN", "Title", "Edition", "Year", "Author", "Bind", "Qty", "Price", "Publisher"];
                        for (int i = 0; i < headers.Length; i++)
                        {
                            worksheet.Cell(1, i + 1).Value = headers[i];
                        }
                        worksheet.Column(2).Style.NumberFormat.Format = "0";

                        worksheet.Cell(2, 1).Value = "Science";
                        worksheet.Cell(2, 2).Value = 9781233999550;
                        worksheet.Cell(2, 3).Value = "Sample Book";
                        worksheet.Cell(2, 4).Value = "1st";
                        worksheet.Cell(2, 5).Value = 2024;
                        worksheet.Cell(2, 6).Value = "John Doe";
                        worksheet.Cell(2, 7).Value = "Hardbound";
                        worksheet.Cell(2, 8).Value = 10;
                        worksheet.Cell(2, 9).Value = 450.00;
                        worksheet.Cell(2, 10).Value = "Sample Publisher";

                        worksheet.Range("A1:J1").Style.Font.Bold = true;
                        worksheet.Columns().AdjustToContents();

                        workbook.SaveAs(sfd.FileName);
                    }

                    MessageBox.Show("True Excel Template generated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    Process.Start(new ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to generate template: " + ex.Message);
                }
            }
        }

        private async void BtnDeleteBook_Click(object sender, EventArgs e)
        {
            if (dgvBookList.SelectedRows.Count > 0)
            {
                var row = dgvBookList.SelectedRows[0];
                string isbn = row.Cells["ISBN"].Value.ToString()!;
                string title = row.Cells["Title"].Value.ToString()!;
                int lastQty = Convert.ToInt32(row.Cells["Qty"].Value);

                if (MessageBox.Show($"Delete '{title}'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    await Task.Run(() =>
                    {
                        DBConnection.DeleteBook(isbn!);
                        DBConnection.LogTransaction(
                            isbn!,
                            title!,
                            -lastQty,
                            "0",
                            "Book Deleted",
                            CurrentSession.User!.DisplayName);
                    });

                    await RefreshBookList();
                }
            }
        }

        private void BtnScanBook_Click(object sender, EventArgs e)
        {
            BookScanner bookScanner = new();
            bookScanner.Show();
        }

        private void DgvBookList_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
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

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            _searchDebouncer.Stop();
            _searchDebouncer.Start();
        }

        private void LblSuggestion_Click(object sender, EventArgs e)
        {
            string suggestedWord = lblSuggestion.Text
            .Replace("Did you mean: ", "")
            .Replace("?", "")
            .Trim();

            txtSearch.Text = suggestedWord;
        }

        private void DgvBookList_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewColumn column in dgvBookList.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }
    }
}