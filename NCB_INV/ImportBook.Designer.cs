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
            dgvBookList = new DataGridView();
            btnReload = new Button();
            btnImport = new Button();
            btnAddBook = new Button();
            btnModifyBook = new Button();
            btnDeleteBook = new Button();
            btnSearch = new Button();
            txtSearch = new TextBox();
            label2 = new Label();
            btnScanBook = new Button();
            lblSyncStatus = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvBookList).BeginInit();
            SuspendLayout();
            // 
            // dgvBookList
            // 
            dgvBookList.AllowUserToAddRows = false;
            dgvBookList.AllowUserToDeleteRows = false;
            dgvBookList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvBookList.Location = new Point(12, 112);
            dgvBookList.Name = "dgvBookList";
            dgvBookList.ReadOnly = true;
            dgvBookList.RowHeadersWidth = 51;
            dgvBookList.Size = new Size(974, 273);
            dgvBookList.TabIndex = 0;
            // 
            // btnReload
            // 
            btnReload.Location = new Point(992, 12);
            btnReload.Name = "btnReload";
            btnReload.Size = new Size(148, 43);
            btnReload.TabIndex = 1;
            btnReload.Text = "Reload Book List";
            btnReload.UseVisualStyleBackColor = true;
            btnReload.Click += btnReload_Click;
            // 
            // btnImport
            // 
            btnImport.Location = new Point(992, 184);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(148, 43);
            btnImport.TabIndex = 2;
            btnImport.Text = "Import Book List";
            btnImport.UseVisualStyleBackColor = true;
            btnImport.Click += btnImport_Click;
            // 
            // btnAddBook
            // 
            btnAddBook.Location = new Point(992, 135);
            btnAddBook.Name = "btnAddBook";
            btnAddBook.Size = new Size(148, 43);
            btnAddBook.TabIndex = 3;
            btnAddBook.Text = "Add Book";
            btnAddBook.UseVisualStyleBackColor = true;
            btnAddBook.Click += btnAddBook_Click;
            // 
            // btnModifyBook
            // 
            btnModifyBook.Location = new Point(992, 233);
            btnModifyBook.Name = "btnModifyBook";
            btnModifyBook.Size = new Size(148, 43);
            btnModifyBook.TabIndex = 4;
            btnModifyBook.Text = "Modify Book";
            btnModifyBook.UseVisualStyleBackColor = true;
            btnModifyBook.Click += btnModifyBook_Click;
            // 
            // btnDeleteBook
            // 
            btnDeleteBook.Location = new Point(992, 282);
            btnDeleteBook.Name = "btnDeleteBook";
            btnDeleteBook.Size = new Size(148, 43);
            btnDeleteBook.TabIndex = 5;
            btnDeleteBook.Text = "Delete Book";
            btnDeleteBook.UseVisualStyleBackColor = true;
            btnDeleteBook.Click += btnDeleteBook_Click;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(838, 12);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(148, 43);
            btnSearch.TabIndex = 7;
            btnSearch.Text = "Search";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // txtSearch
            // 
            txtSearch.Font = new Font("Segoe UI", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtSearch.Location = new Point(12, 15);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(820, 38);
            txtSearch.TabIndex = 8;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(437, 68);
            label2.Name = "label2";
            label2.Size = new Size(170, 41);
            label2.TabIndex = 9;
            label2.Text = "BOOK LIST";
            // 
            // btnScanBook
            // 
            btnScanBook.Location = new Point(992, 331);
            btnScanBook.Name = "btnScanBook";
            btnScanBook.Size = new Size(148, 43);
            btnScanBook.TabIndex = 10;
            btnScanBook.Text = "Scan Books";
            btnScanBook.UseVisualStyleBackColor = true;
            btnScanBook.Click += btnScanBook_Click;
            // 
            // lblSyncStatus
            // 
            lblSyncStatus.AutoSize = true;
            lblSyncStatus.Font = new Font("Segoe UI", 16.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblSyncStatus.Location = new Point(17, 398);
            lblSyncStatus.Name = "lblSyncStatus";
            lblSyncStatus.Size = new Size(92, 38);
            lblSyncStatus.TabIndex = 11;
            lblSyncStatus.Text = "Ready";
            // 
            // ImportBook
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1146, 449);
            Controls.Add(lblSyncStatus);
            Controls.Add(btnScanBook);
            Controls.Add(label2);
            Controls.Add(txtSearch);
            Controls.Add(btnSearch);
            Controls.Add(btnDeleteBook);
            Controls.Add(btnModifyBook);
            Controls.Add(btnAddBook);
            Controls.Add(btnImport);
            Controls.Add(btnReload);
            Controls.Add(dgvBookList);
            Name = "ImportBook";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "INVENTORY";
            Load += ImportBook_Load;
            ((System.ComponentModel.ISupportInitialize)dgvBookList).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvBookList;
        private Button btnReload;
        private Button btnImport;
        private Button btnAddBook;
        private Button btnModifyBook;
        private Button btnDeleteBook;
        private Button btnSearch;
        private TextBox txtSearch;
        private Label label2;
        private Button btnScanBook;
        private Label lblSyncStatus;
    }
}