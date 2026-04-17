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
        public Book BookData { get; set; }
        private bool isEditMode = false;
        public BookEditorForm(Book existingbook = null)
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            if (existingbook != null) {

                //EDIT
                isEditMode = true;
                this.BookData = existingbook;
                PopulateFields();


                txtISBN.ReadOnly = true;
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
            txtISBN.Text = BookData.ISBN;
            txtTitle.Text = BookData.Title;
            txtEdition.Text = BookData.Edition;
            txtYear.Text = BookData.Year;
            txtAuthor.Text = BookData.Author;
            txtBind.Text = BookData.Bind;
            txtQty.Text = BookData.Qty.ToString();
            txtPrice.Text = BookData.Price.ToString();
            txtPublisher.Text = BookData.Publisher;
        }

        private void BookEditorForm_Load(object sender, EventArgs e)
        {

        }

        private void btnAddBook_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtISBN.Text) && string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("ISBN/Title is required!");
                return;
            }

            BookData = new Book(
                txtISBN.Text,
                txtTitle.Text,
                txtEdition.Text,
                txtYear.Text,
                txtAuthor.Text,
                txtBind.Text,
                int.Parse(txtQty.Text),
                decimal.Parse(txtPrice.Text),
                txtPublisher.Text
            );

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
