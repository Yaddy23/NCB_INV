using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

                int isbnW = 15;
                int titleW = 25;
                int oldQtyW = 10;
                int newQtyW = 10;

                StringBuilder report = new StringBuilder();
                report.AppendLine("============================================================");
                report.AppendLine("                BULK QUANTITY UPDATE REPORT                 ");
                report.AppendLine($"                Date: {DateTime.Now.ToString("f")}");
                report.AppendLine("============================================================");

                report.AppendLine(string.Format("{0,-15} | {1,-25} | {2,-10} | {3,-10}", "ISBN", "Title", "Old Qty", "New Qty"));
                report.AppendLine(new string('-', 15 + 3 + 25 + 3 + 10 + 3 + 10));

                int updatedCount = 0;
                int failCount = 0;

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

                                    string cleanTitle = book.Title.Replace("\r", "").Replace("\n", " ").Replace("\t", " ").Trim();
                                    if (cleanTitle.Length > (titleW - 3))
                                        cleanTitle = cleanTitle.Substring(0, titleW - 3) + "..";

                                    string formattedTitle = cleanTitle.PadRight(titleW);


                                    report.AppendLine(string.Format("{0,-" + isbnW + "} | {1} | {2,-" + oldQtyW + "} | {3,-" + newQtyW + "}",
                                        book.ISBN,
                                        formattedTitle,
                                        oldQty,
                                        book.Qty));

                                    updatedCount++;
                                }
                                else
                                {
                                    report.AppendLine(string.Format("{0,-" + isbnW + "} | {1} | {2,-" + newQtyW + "}",
                                        isbn, "NOT FOUND IN DATABASE", "N/A"));

                                    failCount++;
                                }
                            }
                        }
                    }

                    report.AppendLine("------------------------------------------------------------");
                    report.AppendLine($"Total Successfully Updated: {updatedCount}");
                    report.AppendLine($"Total Failed (Not in DB):   {failCount}");
                    report.AppendLine("============================================================");

                    string reportPath = Path.Combine(Application.StartupPath, $"SCANNEDBOOKS-UpdateReport_{DateTime.Now:yyyyMMdd_HHmm}.txt");
                    File.WriteAllText(reportPath, report.ToString());

                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(reportPath) { UseShellExecute = true });

                    MessageBox.Show("Import Complete. Report generated.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error processing file: " + ex.Message);
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

                int isbnW = 15;
                int titleW = 25;
                int oldQtyW = 10;
                int newQtyW = 10;

                StringBuilder report = new StringBuilder();
                report.AppendLine("============================================================");
                report.AppendLine("                BULK RELEASE UPDATE REPORT                  ");
                report.AppendLine($"                SCHOOL: {excelName}");
                report.AppendLine($"                Date: {DateTime.Now.ToString("f")}");
                report.AppendLine("============================================================");

                report.AppendLine(string.Format("{0,-15} | {1,-25} | {2,-10} | {3,-10}", "ISBN", "Title", "Old Qty", "New Qty"));
                report.AppendLine(new string('-', 15 + 3 + 25 + 3 + 10 + 3 + 10));

                int updatedCount = 0;
                int failCount = 0;

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

                                    string cleanTitle = book.Title.Replace("\r", "").Replace("\n", " ").Replace("\t", " ").Trim();
                                    if (cleanTitle.Length > (titleW - 3))
                                        cleanTitle = cleanTitle.Substring(0, titleW - 3) + "..";

                                    string formattedTitle = cleanTitle.PadRight(titleW);


                                    report.AppendLine(string.Format("{0,-" + isbnW + "} | {1} | {2,-" + oldQtyW + "} | {3,-" + newQtyW + "}",
                                        book.ISBN,
                                        formattedTitle,
                                        oldQty,
                                        book.Qty));

                                    updatedCount++;
                                }
                                else
                                {
                                    report.AppendLine(string.Format("{0,-" + isbnW + "} | {1} | {2,-" + newQtyW + "}",
                                        isbn, "NOT FOUND IN DATABASE", "N/A"));

                                    failCount++;
                                }
                            }
                        }
                    }

                    report.AppendLine("------------------------------------------------------------");
                    report.AppendLine($"Total Successfully Updated: {updatedCount}");
                    report.AppendLine($"Total Failed (Not in DB):   {failCount}");
                    report.AppendLine("============================================================");

                    string docpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                    string reportPath = Path.Combine(docpath, $"RELEASE-{excelName}_{DateTime.Now:yyyyMMdd_HHmm}.txt");
                    File.WriteAllText(reportPath, report.ToString());

                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(reportPath) { UseShellExecute = true });

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error processing file: " + ex.Message);
                }
            }
        }
    }
}
