namespace NCB_INV
{
    partial class ImportBook
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
            dgrid1 = new DataGridView();
            btnReload = new Button();
            btnImport = new Button();
            btnAddBook = new Button();
            btnModifyBook = new Button();
            btnDeleteBook = new Button();
            label1 = new Label();
            ((System.ComponentModel.ISupportInitialize)dgrid1).BeginInit();
            SuspendLayout();
            // 
            // dgrid1
            // 
            dgrid1.AllowUserToAddRows = false;
            dgrid1.AllowUserToDeleteRows = false;
            dgrid1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgrid1.Location = new Point(12, 12);
            dgrid1.Name = "dgrid1";
            dgrid1.ReadOnly = true;
            dgrid1.RowHeadersWidth = 51;
            dgrid1.Size = new Size(566, 245);
            dgrid1.TabIndex = 0;
            // 
            // btnReload
            // 
            btnReload.Location = new Point(584, 15);
            btnReload.Name = "btnReload";
            btnReload.Size = new Size(148, 43);
            btnReload.TabIndex = 1;
            btnReload.Text = "Reload Book List";
            btnReload.UseVisualStyleBackColor = true;
            // 
            // btnImport
            // 
            btnImport.Location = new Point(584, 64);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(148, 43);
            btnImport.TabIndex = 2;
            btnImport.Text = "Import Book List";
            btnImport.UseVisualStyleBackColor = true;
            // 
            // btnAddBook
            // 
            btnAddBook.Location = new Point(584, 113);
            btnAddBook.Name = "btnAddBook";
            btnAddBook.Size = new Size(148, 43);
            btnAddBook.TabIndex = 3;
            btnAddBook.Text = "Add Book";
            btnAddBook.UseVisualStyleBackColor = true;
            // 
            // btnModifyBook
            // 
            btnModifyBook.Location = new Point(584, 162);
            btnModifyBook.Name = "btnModifyBook";
            btnModifyBook.Size = new Size(148, 43);
            btnModifyBook.TabIndex = 4;
            btnModifyBook.Text = "Modify Book";
            btnModifyBook.UseVisualStyleBackColor = true;
            // 
            // btnDeleteBook
            // 
            btnDeleteBook.Location = new Point(584, 211);
            btnDeleteBook.Name = "btnDeleteBook";
            btnDeleteBook.Size = new Size(148, 43);
            btnDeleteBook.TabIndex = 5;
            btnDeleteBook.Text = "Delete Book";
            btnDeleteBook.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(12, 270);
            label1.Name = "label1";
            label1.Size = new Size(143, 41);
            label1.TabIndex = 6;
            label1.Text = "STATUS: ";
            // 
            // ImportBook
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(744, 321);
            Controls.Add(label1);
            Controls.Add(btnDeleteBook);
            Controls.Add(btnModifyBook);
            Controls.Add(btnAddBook);
            Controls.Add(btnImport);
            Controls.Add(btnReload);
            Controls.Add(dgrid1);
            Name = "ImportBook";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ImportBook";
            Load += ImportBook_Load;
            ((System.ComponentModel.ISupportInitialize)dgrid1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgrid1;
        private Button btnReload;
        private Button btnImport;
        private Button btnAddBook;
        private Button btnModifyBook;
        private Button btnDeleteBook;
        private Label label1;
    }
}