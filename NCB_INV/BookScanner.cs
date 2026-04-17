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
                StringBuilder report = new StringBuilder();
                report.AppendLine("===========================================");
                report.AppendLine("       BULK QUANTITY UPDATE REPORT         ");
                report.AppendLine($"       Date: {DateTime.Now.ToString("f")}");
                report.AppendLine("===========================================");
                report.AppendLine(string.Format("{0,-15} | {1,-30} | {2,-10}", "ISBN", "Title", "New Qty"));
                report.AppendLine("------------------------------------------------------------");

                int updatedCount = 0;
                int failCount = 0;

                using (var stream = File.Open(ofd.FileName, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet();
                        var table = result.Tables[0];

                        for (int i = 1; i < table.Rows.Count; i++)
                        {
                            string isbn = table.Rows[i][0].ToString().Trim();
                            if (string.IsNullOrEmpty(isbn)) continue;

                            Book book = DBConnection.GetBookByISBN(isbn);

                            if (book != null)
                            {
                              
                                book.Qty += 1;
                                DBConnection.SaveBook(book);

                              
                                report.AppendLine(string.Format("{0,-15} | {1,-30} | {2,-10}",
                                    book.ISBN,
                                    book.Title.Length > 28 ? book.Title.Substring(0, 27) + ".." : book.Title,
                                    book.Qty));
                                updatedCount++;
                            }
                            else
                            {
                              
                                report.AppendLine(string.Format("{0,-15} | {1,-30} | {2,-10}",
                                    isbn, "NOT FOUND IN DATABASE", "N/A"));
                                failCount++;
                            }
                        }
                    }
                }

                report.AppendLine("------------------------------------------------------------");
                report.AppendLine($"Total Successfully Updated: {updatedCount}");
                report.AppendLine($"Total Failed (Not in DB): {failCount}");
                report.AppendLine("===========================================");

                string reportPath = Path.Combine(Application.StartupPath, $"UpdateReport_{DateTime.Now:yyyyMMdd_HHmm}.txt");
                File.WriteAllText(reportPath, report.ToString());
                
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(reportPath) { UseShellExecute = true });

                MessageBox.Show("Import Complete. Report has been generated.");
            }
        }
    }
}
