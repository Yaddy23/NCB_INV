using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using ExcelDataReader;
using System.Data.Common;

namespace NCB_INV
{
    public partial class BookScanner : Form
    {
        private Book? currentbook;

        public BookScanner()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ActiveControl = txtBarcodeScanner;
        }

        private void txtBarcodeScanner_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string isbn = txtBarcodeScanner.Text.Trim();
                if (string.IsNullOrEmpty(isbn)) return;

                currentbook = DBConnection.GetBookByISBN(isbn);
                string currentuser = DBConnection.CurrentSession.User?.DisplayName ?? "Unknown User";

                if (currentbook != null)
                {
                    int oldQty = currentbook.Qty;
                    currentbook.Qty += 1;
                    DBConnection.SaveBook(currentbook);

                    lblTitle.Text = $"Title: {currentbook.Title}";
                    lblOldQty.Text = $"Previous: {oldQty}";
                    lblNewQty.Text = $"New Total: {currentbook.Qty}";
                    lblNewQty.ForeColor = Color.Green;
                    DBConnection.LogTransaction(txtBarcodeScanner.Text, lblTitle.Text, oldQty, currentbook.Qty.ToString(), "Initial Stock Entry", currentuser);
                }
                else
                {
                    lblTitle.Text = "NOT FOUND";
                    lblOldQty.Text = "";
                    lblNewQty.Text = "No record for this ISBN";
                    lblNewQty.ForeColor = Color.Red;
                }

                txtBarcodeScanner.Clear();
                txtBarcodeScanner.Focus();
                e.SuppressKeyPress = true;
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            ProcessExcelBulkUpdate(true); // stock-in
        }

        private void btnOut_Click(object sender, EventArgs e)
        {
            ProcessExcelBulkUpdate(false);// release
        }

        private static void ProcessExcelBulkUpdate(bool isStockIn)
        {
            OpenFileDialog ofd = new() { Filter = "Excel Files|*.xlsx;*.xls" };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string excelName = Path.GetFileNameWithoutExtension(ofd.FileName);
                int updatedCount = 0;
                int failCount = 0;

                StringBuilder tableRows = new();
                var bulkList = new List<Book>();

                try
                {
                    Cursor.Current = Cursors.WaitCursor;

                    using (var stream = File.Open(ofd.FileName, FileMode.Open, FileAccess.Read))
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet();
                        var table = result.Tables[0];

                        var groupedData = table.AsEnumerable()
                            .Skip(1)
                            .Where(r => !string.IsNullOrEmpty(r[0]?.ToString()))
                            .GroupBy(r => r[0]?.ToString()?.Trim() ?? string.Empty);

                        foreach (var group in groupedData)
                        {
                            string isbn = group.Key;
                            int countInExcel = group.Count();

                            Book? book = DBConnection.GetLocalBookByISBN(isbn);

                            if (book != null)
                            {
                                int totalChange = isStockIn ? countInExcel : -countInExcel;

                                book.Qty += totalChange;
                                bulkList.Add(book);
                                updatedCount += countInExcel;

                                tableRows.AppendLine($@"
                        <tr>
                            <td>{book.ISBN}</td>
                            <td>{book.Title}</td>
                            <td style='color: {(isStockIn ? "green" : "blue")}; font-weight: bold;'>{(isStockIn ? "+" : "-")}{countInExcel}</td>
                        </tr>");

                                if (bulkList.Count >= 5000)
                                {
                                    DBConnection.SyncBookQuantitiesLocal(bulkList);
                                    bulkList.Clear();
                                }
                            }
                            else
                            {
                                // Still fail if the ISBN doesn't exist in the database at all
                                tableRows.AppendLine($"<tr style='background-color: #ffe6e6;'><td>{isbn}</td><td style='color: red;'>NOT FOUND</td><td>{countInExcel}</td></tr>");
                                failCount += countInExcel;
                            }
                        }
                    }

                    if (bulkList.Count > 0) { DBConnection.SyncBookQuantitiesLocal(bulkList); }

                    if (updatedCount > 0)
                    {
                        string displayName = DBConnection.CurrentSession.User?.DisplayName ?? "Warehouse User";

                        DBConnection.LogTransaction(
                            "BULK_IMPORT",
                            $"Excel: {excelName}",
                            isStockIn ? updatedCount : -updatedCount,
                            "Report Generated",
                            isStockIn ? "Bulk Stock-In" : "Bulk Release",
                            displayName
                        );
                    }

                    string reportTitle = isStockIn ? "BULK STOCK-IN" : "BULK RELEASE";
                    string finalHtml = GetHtmlTemplate(excelName, tableRows.ToString(), updatedCount, failCount, reportTitle);
                    GenerateAndOpenReport(finalHtml, isStockIn);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Process Error: " + ex.Message);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private static void GenerateAndOpenReport(string html, bool isStockIn)
        {
            string folderName = isStockIn ? "StockIn_Reports" : "Release_Reports";
            string targetFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NCB_INVENTORY", folderName);

            if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

            string prefix = isStockIn ? "IN" : "OUT";
            string filePath = Path.Combine(targetFolder, $"{prefix}_{DateTime.Now:yyyyMMdd_HHmm}.html");

            File.WriteAllText(filePath, html);
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }

        private static string GetHtmlTemplate(string source, string rows, int success, int fails, string type)
        {
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ncblogo.png");
            string logoUri = File.Exists(logoPath) ? new Uri(logoPath).AbsoluteUri : "";

            return $@"
    <html>
    <head>
        <style>
            body {{ font-family: 'Segoe UI', sans-serif; margin: 40px; color: #333; }}
            .header {{ display: flex; align-items: center; border-bottom: 3px solid #2c3e50; padding-bottom: 15px; }}
            .logo {{ width: 80px; margin-right: 20px; }}
            h1 {{ margin: 0; color: #2c3e50; }}
            table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
            th {{ background: #2c3e50; color: white; padding: 10px; text-align: left; }}
            td {{ padding: 10px; border-bottom: 1px solid #ddd; }}
            .summary {{ margin-top: 20px; padding: 15px; background: #f9f9f9; border: 1px solid #ddd; }}
        </style>
    </head>
    <body>
        <div class='header'>
            <img src='{logoUri}' class='logo' />
            <div>
                <h1>{type} REPORT</h1>
                <small>Source: MongoDB Atlas | File: {source} | Date: {DateTime.Now:g}</small>
            </div>
        </div>
        <table>
            <thead><tr><th>ISBN</th><th>Title</th><th>Qty Change</th></tr></thead>
            <tbody>{rows}</tbody>
        </table>
        <div class='summary'>
            <strong>Total Processed:</strong> {success} | <strong>Failed/Missing:</strong> {fails}
        </div>
    </body>
    </html>";
        }

        private void btnClose_Click(object sender, EventArgs e) => this.Close();

    }
}