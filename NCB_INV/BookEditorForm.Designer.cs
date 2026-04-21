namespace NCB_INV
{
    partial class BookEditorForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtISBN = new TextBox();
            txtTitle = new TextBox();
            txtEdition = new TextBox();
            txtYear = new TextBox();
            txtAuthor = new TextBox();
            txtBind = new TextBox();
            txtQty = new TextBox();
            txtPrice = new TextBox();
            txtPublisher = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            label8 = new Label();
            label9 = new Label();
            btnAddBook = new Button();
            btnCancel = new Button();
            lblStatus = new Label();
            SuspendLayout();
            // 
            // txtISBN
            // 
            txtISBN.Location = new Point(141, 12);
            txtISBN.Name = "txtISBN";
            txtISBN.Size = new Size(228, 27);
            txtISBN.TabIndex = 0;
            txtISBN.KeyDown += txtISBN_KeyDown;
            // 
            // txtTitle
            // 
            txtTitle.Location = new Point(141, 45);
            txtTitle.Name = "txtTitle";
            txtTitle.Size = new Size(228, 27);
            txtTitle.TabIndex = 1;
            // 
            // txtEdition
            // 
            txtEdition.Location = new Point(141, 78);
            txtEdition.Name = "txtEdition";
            txtEdition.Size = new Size(228, 27);
            txtEdition.TabIndex = 2;
            // 
            // txtYear
            // 
            txtYear.Location = new Point(141, 111);
            txtYear.Name = "txtYear";
            txtYear.Size = new Size(228, 27);
            txtYear.TabIndex = 3;
            // 
            // txtAuthor
            // 
            txtAuthor.Location = new Point(141, 144);
            txtAuthor.Name = "txtAuthor";
            txtAuthor.Size = new Size(228, 27);
            txtAuthor.TabIndex = 4;
            // 
            // txtBind
            // 
            txtBind.Location = new Point(141, 177);
            txtBind.Name = "txtBind";
            txtBind.Size = new Size(228, 27);
            txtBind.TabIndex = 5;
            // 
            // txtQty
            // 
            txtQty.Location = new Point(141, 210);
            txtQty.Name = "txtQty";
            txtQty.Size = new Size(228, 27);
            txtQty.TabIndex = 6;
            // 
            // txtPrice
            // 
            txtPrice.Location = new Point(141, 243);
            txtPrice.Name = "txtPrice";
            txtPrice.Size = new Size(228, 27);
            txtPrice.TabIndex = 7;
            // 
            // txtPublisher
            // 
            txtPublisher.Location = new Point(141, 276);
            txtPublisher.Name = "txtPublisher";
            txtPublisher.Size = new Size(228, 27);
            txtPublisher.TabIndex = 8;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(25, 15);
            label1.Name = "label1";
            label1.Size = new Size(44, 20);
            label1.TabIndex = 9;
            label1.Text = "ISBN:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(25, 48);
            label2.Name = "label2";
            label2.Size = new Size(47, 20);
            label2.TabIndex = 11;
            label2.Text = "TITLE:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(25, 81);
            label3.Name = "label3";
            label3.Size = new Size(69, 20);
            label3.TabIndex = 13;
            label3.Text = "EDITION:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(25, 114);
            label4.Name = "label4";
            label4.Size = new Size(47, 20);
            label4.TabIndex = 15;
            label4.Text = "YEAR:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(25, 147);
            label5.Name = "label5";
            label5.Size = new Size(71, 20);
            label5.TabIndex = 17;
            label5.Text = "AUTHOR:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(25, 180);
            label6.Name = "label6";
            label6.Size = new Size(47, 20);
            label6.TabIndex = 19;
            label6.Text = "BIND:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(25, 213);
            label7.Name = "label7";
            label7.Size = new Size(38, 20);
            label7.TabIndex = 21;
            label7.Text = "QTY:";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(25, 246);
            label8.Name = "label8";
            label8.Size = new Size(50, 20);
            label8.TabIndex = 23;
            label8.Text = "PRICE:";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(25, 279);
            label9.Name = "label9";
            label9.Size = new Size(86, 20);
            label9.TabIndex = 25;
            label9.Text = "PUBLISHER:";
            // 
            // btnAddBook
            // 
            btnAddBook.Location = new Point(385, 15);
            btnAddBook.Name = "btnAddBook";
            btnAddBook.Size = new Size(114, 185);
            btnAddBook.TabIndex = 26;
            btnAddBook.Text = "Add Book";
            btnAddBook.UseVisualStyleBackColor = true;
            btnAddBook.Click += btnAddBook_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(385, 246);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(114, 53);
            btnCancel.TabIndex = 27;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(25, 329);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(62, 20);
            lblStatus.TabIndex = 28;
            lblStatus.Text = "STATUS:";
            // 
            // BookEditorForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(511, 358);
            Controls.Add(lblStatus);
            Controls.Add(btnCancel);
            Controls.Add(btnAddBook);
            Controls.Add(label9);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtPublisher);
            Controls.Add(txtPrice);
            Controls.Add(txtQty);
            Controls.Add(txtBind);
            Controls.Add(txtAuthor);
            Controls.Add(txtYear);
            Controls.Add(txtEdition);
            Controls.Add(txtTitle);
            Controls.Add(txtISBN);
            Name = "BookEditorForm";
            Text = "BookEditorForm";
            Load += BookEditorForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtISBN;
        private TextBox txtTitle;
        private TextBox txtEdition;
        private TextBox txtYear;
        private TextBox txtAuthor;
        private TextBox txtBind;
        private TextBox txtQty;
        private TextBox txtPrice;
        private TextBox txtPublisher;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private Label label9;
        private Button btnAddBook;
        private Button btnCancel;
        private Label lblStatus;
    }
}