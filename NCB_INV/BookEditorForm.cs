using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NCB_INV
{
    public partial class BookEditorForm : Form
    {
        public Book? BookData { get; set; }
        private bool isEditMode;
        public BookEditorForm(Book? existingbook = null)
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            if (existingbook != null)
            {

                //EDIT
                isEditMode = true;
                this.BookData = existingbook;
                PopulateFields();


                txtISBN.ReadOnly = true;
                btnAddBook.Text = "Save Changes";
                this.Text = "Modify Book Details";
            }
            else
            {
                // ADD
                isEditMode = false;
                this.Text = "Add New Book";
            }
        }

        private void PopulateFields()
        {
            txtSubject.Text = BookData?.Subject;
            txtISBN.Text = BookData?.ISBN;
            txtTitle.Text = BookData?.Title;
            txtEdition.Text = BookData?.Edition;
            txtYear.Text = BookData?.Year;
            txtAuthor.Text = BookData?.AuthorId;
            txtBind.Text = BookData?.Bind;
            txtQty.Text = BookData?.Qty.ToString();
            txtPrice.Text = BookData?.Price.ToString();
            txtPublisher.Text = BookData?.PublisherId;
        }

        private void BookEditorForm_Load(object sender, EventArgs e)
        {

        }

        private void btnAddBook_Click(object sender, EventArgs e)
        {
            // 1. Existing text check
            if (string.IsNullOrWhiteSpace(txtISBN.Text) || string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("ISBN and Title are required!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Defensive Quantity Check
            if (!int.TryParse(txtQty.Text, out int qty) || qty < 0)
            {
                MessageBox.Show("Please enter a valid, non-negative whole number for Quantity.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. Defensive Price Check
            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Please enter a valid, non-negative number for Price.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string currentuser = DBConnection.CurrentSession.User?.Username ?? "Unknown User";

            if (isEditMode)
            {
                if (BookData != null)
                {
                    BookData.Subject = txtSubject.Text;
                    BookData.Title = txtTitle.Text;
                    BookData.Edition = txtEdition.Text;
                    BookData.Year = txtYear.Text;
                    BookData.AuthorId = txtAuthor.Text;
                    BookData.Bind = txtBind.Text;
                    BookData.Qty = qty;
                    BookData.Price = price;
                    BookData.PublisherId = txtPublisher.Text;
                    BookData.LastModified = DateTime.Now;
                }
                DBConnection.LogTransaction(txtISBN.Text, txtTitle.Text, qty, $"Updated stock to {qty}", "Book Details Modified", currentuser);
            }
            else
            {
                BookData = new Book(
                    txtSubject.Text,
                    txtISBN.Text,
                    txtTitle.Text,
                    txtEdition.Text,
                    txtYear.Text,
                    txtAuthor.Text,
                    txtBind.Text,
                    qty,
                    price,
                    txtPublisher.Text,
                    DateTime.Now
                );

                DBConnection.LogTransaction(txtISBN.Text, txtTitle.Text, qty, txtQty.Text, "Initial Stock Entry", currentuser);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private async void txtISBN_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string isbn = txtISBN.Text.Trim();

                if (isbn.Length >= 10)
                {
                    e.SuppressKeyPress = true;

                    lblStatus.Text = "Scanning Cloud...";
                    Cursor.Current = Cursors.WaitCursor;

                    try
                    {
                        Book? foundBook = await DBConnection.ScrapeBookData(isbn);

                        if (foundBook != null)
                        {
                            txtSubject.Text = foundBook.Subject;
                            txtTitle.Text = foundBook.Title;
                            txtEdition.Text = foundBook.Edition;
                            txtYear.Text = foundBook.Year;
                            txtAuthor.Text = foundBook.AuthorId;
                            txtBind.Text = foundBook.Bind;
                            txtPublisher.Text = foundBook.PublisherId;

                            txtPrice.Focus();
                            lblStatus.Text = "Stats: Success!";
                        }
                        else
                        {
                            lblStatus.Text = "Status: Not found. Please type details.";
                        }
                    }
                    catch (Exception ex)
                    {
                        lblStatus.Text = "Error: " + ex.Message;
                    }
                    finally
                    {
                        Cursor.Current = Cursors.Default;
                    }
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
