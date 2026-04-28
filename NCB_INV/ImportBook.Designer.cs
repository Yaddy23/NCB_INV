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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
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
            lblNetStatus = new Label();
            flowLayoutPanel_Buttons = new FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)dgvBookList).BeginInit();
            flowLayoutPanel_Buttons.SuspendLayout();
            SuspendLayout();
            // 
            // dgvBookList
            // 
            dgvBookList.AllowUserToAddRows = false;
            dgvBookList.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(249, 249, 249);
            dgvBookList.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvBookList.BackgroundColor = Color.White;
            dgvBookList.BorderStyle = BorderStyle.None;
            dgvBookList.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.Navy;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvBookList.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvBookList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvBookList.Location = new Point(12, 149);
            dgvBookList.Name = "dgvBookList";
            dgvBookList.ReadOnly = true;
            dgvBookList.RowHeadersWidth = 51;
            dgvBookList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBookList.Size = new Size(1138, 273);
            dgvBookList.TabIndex = 0;
            dgvBookList.CellFormatting += dgvBookList_CellFormatting;
            // 
            // btnReload
            // 
            btnReload.Location = new Point(992, 49);
            btnReload.Name = "btnReload";
            btnReload.Size = new Size(148, 43);
            btnReload.TabIndex = 1;
            btnReload.Text = "Reload Book List";
            btnReload.UseVisualStyleBackColor = true;
            btnReload.Click += btnReload_Click;
            // 
            // btnImport
            // 
            btnImport.Location = new Point(5, 5);
            btnImport.Margin = new Padding(5);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(148, 43);
            btnImport.TabIndex = 2;
            btnImport.Text = "Import Book List";
            btnImport.UseVisualStyleBackColor = true;
            btnImport.Click += btnImport_Click;
            // 
            // btnAddBook
            // 
            btnAddBook.Location = new Point(163, 5);
            btnAddBook.Margin = new Padding(5);
            btnAddBook.Name = "btnAddBook";
            btnAddBook.Size = new Size(148, 43);
            btnAddBook.TabIndex = 3;
            btnAddBook.Text = "Add Book";
            btnAddBook.UseVisualStyleBackColor = true;
            btnAddBook.Click += btnAddBook_Click;
            // 
            // btnModifyBook
            // 
            btnModifyBook.Location = new Point(321, 5);
            btnModifyBook.Margin = new Padding(5);
            btnModifyBook.Name = "btnModifyBook";
            btnModifyBook.Size = new Size(148, 43);
            btnModifyBook.TabIndex = 4;
            btnModifyBook.Text = "Modify Book";
            btnModifyBook.UseVisualStyleBackColor = true;
            btnModifyBook.Click += btnModifyBook_Click;
            // 
            // btnDeleteBook
            // 
            btnDeleteBook.Location = new Point(479, 5);
            btnDeleteBook.Margin = new Padding(5);
            btnDeleteBook.Name = "btnDeleteBook";
            btnDeleteBook.Size = new Size(148, 43);
            btnDeleteBook.TabIndex = 5;
            btnDeleteBook.Text = "Delete Book";
            btnDeleteBook.UseVisualStyleBackColor = true;
            btnDeleteBook.Click += btnDeleteBook_Click;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(838, 49);
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
            txtSearch.Location = new Point(12, 52);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(820, 38);
            txtSearch.TabIndex = 8;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(437, 105);
            label2.Name = "label2";
            label2.Size = new Size(170, 41);
            label2.TabIndex = 9;
            label2.Text = "BOOK LIST";
            // 
            // btnScanBook
            // 
            btnScanBook.Location = new Point(637, 5);
            btnScanBook.Margin = new Padding(5);
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
            lblSyncStatus.Location = new Point(17, 494);
            lblSyncStatus.Name = "lblSyncStatus";
            lblSyncStatus.Size = new Size(0, 38);
            lblSyncStatus.TabIndex = 11;
            // 
            // lblNetStatus
            // 
            lblNetStatus.AutoSize = true;
            lblNetStatus.Font = new Font("Segoe UI", 16.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblNetStatus.Location = new Point(17, 9);
            lblNetStatus.Name = "lblNetStatus";
            lblNetStatus.Size = new Size(0, 38);
            lblNetStatus.TabIndex = 13;
            // 
            // flowLayoutPanel_Buttons
            // 
            flowLayoutPanel_Buttons.Controls.Add(btnImport);
            flowLayoutPanel_Buttons.Controls.Add(btnAddBook);
            flowLayoutPanel_Buttons.Controls.Add(btnModifyBook);
            flowLayoutPanel_Buttons.Controls.Add(btnDeleteBook);
            flowLayoutPanel_Buttons.Controls.Add(btnScanBook);
            flowLayoutPanel_Buttons.Location = new Point(186, 428);
            flowLayoutPanel_Buttons.Name = "flowLayoutPanel_Buttons";
            flowLayoutPanel_Buttons.Size = new Size(800, 54);
            flowLayoutPanel_Buttons.TabIndex = 14;
            flowLayoutPanel_Buttons.WrapContents = false;
            // 
            // ImportBook
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1157, 541);
            Controls.Add(flowLayoutPanel_Buttons);
            Controls.Add(lblNetStatus);
            Controls.Add(lblSyncStatus);
            Controls.Add(label2);
            Controls.Add(txtSearch);
            Controls.Add(btnSearch);
            Controls.Add(btnReload);
            Controls.Add(dgvBookList);
            Name = "ImportBook";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "INVENTORY";
            FormClosing += ImportBook_FormClosing;
            Load += ImportBook_Load;
            ((System.ComponentModel.ISupportInitialize)dgvBookList).EndInit();
            flowLayoutPanel_Buttons.ResumeLayout(false);
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
        private Label lblNetStatus;
        private FlowLayoutPanel flowLayoutPanel_Buttons;
    }
}