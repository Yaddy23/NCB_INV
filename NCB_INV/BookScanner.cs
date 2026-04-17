using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExcelDataReader;

namespace NCB_INV
{
    public partial class BookScanner : Form
    {

        private Book currentbook;
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
                currentbook = DBConnection.GetBookByISBN(isbn);
                if (currentbook != null)
                {

                    int addQTY = 1;

                    currentbook.Qty += addQTY;
                    DBConnection.SaveBook(currentbook);

                    lblTitle.Text = $"Title: {currentbook.Title}";
                    lblOldQty.Text = $"Previous: {currentbook.Qty - addQTY}";
                    lblNewQty.Text = $"New Total: {currentbook.Qty}";
                    lblNewQty.ForeColor = Color.Green;

                    //enable = beep after updating the quantity
                    //System.Media.SystemSounds.Beep.Play();
                }
                else
                {
                    lblTitle.Text = "NOT FOUND";
                    lblNewQty.Text = "No record for this ISBN";
                    lblNewQty.ForeColor = Color.Red;
                }
                txtBarcodeScanner.Clear();
                txtBarcodeScanner.Focus();

                //enable = prevent the beep sound when pressing Enter
                //e.SuppressKeyPress = true;
            }

        }

        private void BookScanner_Load(object sender, EventArgs e)
        {

        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Excel Files|*.xlsx;*.xls" };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string excelName = Path.GetFileNameWithoutExtension(ofd.FileName);
                int updatedCount = 0;
                int failCount = 0;

                StringBuilder tableRows = new StringBuilder();

                try
                {
                    using (var stream = File.Open(ofd.FileName, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var result = reader.AsDataSet();
                            var table = result.Tables[0];

                            for (int i = 1; i < table.Rows.Count; i++)
                            {
                                string isbn = table.Rows[i][0]?.ToString().Trim() ?? "";
                                if (string.IsNullOrEmpty(isbn)) continue;

                                Book book = DBConnection.GetBookByISBN(isbn);

                                if (book != null)
                                {
                                    int oldQty = book.Qty;
                                    book.Qty += 1;
                                    DBConnection.SaveBook(book);

                                    tableRows.AppendLine($@"
                                <tr>
                                    <td>{book.ISBN}</td>
                                    <td>{book.Title}</td>
                                    <td>{oldQty}</td>
                                    <td style='color: green; font-weight: bold;'>{book.Qty}</td>
                                </tr>");
                                        updatedCount++;
                                }
                                else
                                {
                                    tableRows.AppendLine($@"
                                <tr style='background-color: #ffe6e6;'>
                                    <td>{isbn}</td>
                                    <td style='color: red;'>NOT FOUND IN DATABASE</td>
                                    <td>N/A</td>
                                    <td>N/A</td>
                                </tr>");
                                    failCount++;
                                }
                            }
                        }
                    }

                    string finalHtml = GetHtmlTemplate(excelName, tableRows.ToString(), updatedCount, failCount);

                    string targetFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NCB_INVENTORY_Reports");
                    if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

                    string fileName = $"SCANNED BOOKS-_{DateTime.Now:yyyyMMdd_HHmm}.html";
                    string filePath = Path.Combine(targetFolder, fileName);

                    File.WriteAllText(filePath, finalHtml);

                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOut_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Excel Files|*.xlsx;*.xls" };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string excelName = Path.GetFileNameWithoutExtension(ofd.FileName);
                int updatedCount = 0;
                int failCount = 0;

                StringBuilder tableRows = new StringBuilder();

                try
                {
                    using (var stream = File.Open(ofd.FileName, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var result = reader.AsDataSet();
                            var table = result.Tables[0];

                            for (int i = 1; i < table.Rows.Count; i++)
                            {
                                string isbn = table.Rows[i][0]?.ToString().Trim() ?? "";
                                if (string.IsNullOrEmpty(isbn)) continue;

                                Book book = DBConnection.GetBookByISBN(isbn);

                                if (book != null)
                                {
                                    int oldQty = book.Qty;
                                    book.Qty -= 1;
                                    DBConnection.SaveBook(book);

                                    tableRows.AppendLine($@"
                                    <tr>
                                        <td>{book.ISBN}</td>
                                        <td>{book.Title}</td>
                                        <td>{oldQty}</td>
                                        <td style='color: green; font-weight: bold;'>{book.Qty}</td>
                                    </tr>");
                                    updatedCount++;
                                }
                                else
                                {
                                    tableRows.AppendLine($@"
                                    <tr style='background-color: #ffe6e6;'>
                                        <td>{isbn}</td>
                                        <td style='color: red;'>NOT FOUND IN DATABASE</td>
                                        <td>N/A</td>
                                        <td>N/A</td>
                                    </tr>");
                                    failCount++;
                                }
                            }
                        }
                    }

                    string finalHtml = GetHtmlTemplate(excelName, tableRows.ToString(), updatedCount, failCount);

                    string targetFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NCB_INVENTORY_Reports");
                    if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

                    string fileName = $"RELEASED TO-{excelName.ToUpper()}_{DateTime.Now:yyyyMMdd_HHmm}.html";
                    string filePath = Path.Combine(targetFolder, fileName);

                    File.WriteAllText(filePath, finalHtml);

                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
        private string GetHtmlTemplate(string source, string rows, int success, int fails)
        {
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ncblogo.png"); // Make sure your file is named logo.png

            // Convert to a URI format that browsers understand
            string logoUri = new Uri(logoPath).AbsoluteUri;

            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: 'Segoe UI', Arial, sans-serif; margin: 40px; color: #333; }}
                    .header-container {{ display: flex; align-items: center; border-bottom: 3px solid #3498db; padding-bottom: 15px; }}
                    .logo {{ width: 100px; height: auto; margin-right: 25px; }}
                    .header-text {{ flex-grow: 1; }}
                    h1 {{ color: #2c3e50; margin: 0; font-size: 28px; letter-spacing: 1px; }}
                    .meta {{ color: #7f8c8d; font-size: 0.9em; margin-top: 5px; }}
                    table {{ width: 100%; border-collapse: collapse; margin-top: 25px; table-layout: fixed; }}
                    th {{ background-color: #3498db; color: white; padding: 12px; text-align: left; text-transform: uppercase; font-size: 0.85em; }}
                    td {{ padding: 10px; border-bottom: 1px solid #eee; word-wrap: break-word; }}
                    tr:nth-child(even) {{ background-color: #f8f9fa; }}
                    .summary {{ margin-top: 30px; padding: 20px; background: #f1f4f6; border-radius: 8px; border-left: 5px solid #3498db; }}
                    .status-fail {{ background-color: #fff5f5; color: #c0392b; }}
                    .qty-change {{ font-weight: bold; color: #27ae60; }}
                </style>
            </head>
            <body>
                <div class='header-container'>
                    <img src='{logoUri}' class='logo' />
                    <div class='header-text'>
                        <h1>BULK UPDATE REPORT</h1>
                        <div class='meta'>
                            <strong>SOURCE FILE:</strong> {source} <br>
                            <strong>PROCESSED ON:</strong> {DateTime.Now:f}
                        </div>
                    </div>
                </div>

                <table>
                    <thead>
                        <tr>
                            <th style='width: 25%'>ISBN</th>
                            <th style='width: 45%'>Title</th>
                            <th style='width: 15%'>Old Qty</th>
                            <th style='width: 15%'>New Qty</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rows}
                    </tbody>
                </table>

                <div class='summary'>
                    <h3 style='margin-top: 0;'>Update Summary</h3>
                    <p>Successfully Released: <strong>{success}</strong></p>
                    <p>Failed / Not Found: <strong style='color: {(fails > 0 ? "#c0392b" : "#27ae60")};'>{fails}</strong></p>
                </div>
            </body>
            </html>";
        }

    }
}
