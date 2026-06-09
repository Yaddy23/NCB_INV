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
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle7 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle8 = new DataGridViewCellStyle();
            dgvBookList = new DataGridView();
            btnReload = new Button();
            btnImport = new Button();
            btnAddBook = new Button();
            btnModifyBook = new Button();
            btnDeleteBook = new Button();
            txtSearch = new TextBox();
            label2 = new Label();
            btnScanBook = new Button();
            lblSyncStatus = new Label();
            lblNetStatus = new Label();
            flowLayoutPanel_Buttons = new FlowLayoutPanel();
            btnPricelist = new Button();
            lblSuggestion = new Label();
            lblTotalStocks = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvBookList).BeginInit();
            flowLayoutPanel_Buttons.SuspendLayout();
            SuspendLayout();
            // 
            // dgvBookList
            // 
            dgvBookList.AllowUserToAddRows = false;
            dgvBookList.AllowUserToDeleteRows = false;
            dgvBookList.AllowUserToResizeColumns = false;
            dgvBookList.AllowUserToResizeRows = false;
            dataGridViewCellStyle5.BackColor = Color.FromArgb(249, 249, 249);
            dataGridViewCellStyle5.Font = new Font("Microsoft Sans Serif", 13.8F);
            dgvBookList.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle5;
            dgvBookList.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            dgvBookList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBookList.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvBookList.BackgroundColor = Color.White;
            dgvBookList.BorderStyle = BorderStyle.None;
            dgvBookList.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = Color.Navy;
            dataGridViewCellStyle6.Font = new Font("Microsoft Sans Serif", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle6.ForeColor = Color.White;
            dataGridViewCellStyle6.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = DataGridViewTriState.True;
            dgvBookList.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle6;
            dgvBookList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle7.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = SystemColors.Window;
            dataGridViewCellStyle7.Font = new Font("Microsoft Sans Serif", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle7.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle7.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = DataGridViewTriState.False;
            dgvBookList.DefaultCellStyle = dataGridViewCellStyle7;
            dgvBookList.Location = new Point(12, 224);
            dgvBookList.Name = "dgvBookList";
            dgvBookList.ReadOnly = true;
            dataGridViewCellStyle8.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = SystemColors.Control;
            dataGridViewCellStyle8.Font = new Font("Microsoft Sans Serif", 13.8F);
            dataGridViewCellStyle8.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle8.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = DataGridViewTriState.True;
            dgvBookList.RowHeadersDefaultCellStyle = dataGridViewCellStyle8;
            dgvBookList.RowHeadersWidth = 100;
            dgvBookList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBookList.Size = new Size(1849, 675);
            dgvBookList.TabIndex = 0;
            dgvBookList.CellFormatting += dgvBookList_CellFormatting;
            // 
            // btnReload
            // 
            btnReload.Font = new Font("Microsoft Sans Serif", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnReload.Location = new Point(1497, 44);
            btnReload.Name = "btnReload";
            btnReload.Size = new Size(358, 56);
            btnReload.TabIndex = 1;
            btnReload.Text = "RELOAD BOOK LIST";
            btnReload.UseVisualStyleBackColor = true;
            btnReload.Click += btnReload_Click;
            // 
            // btnImport
            // 
            btnImport.Font = new Font("Segoe UI", 16.2F, FontStyle.Bold);
            btnImport.Location = new Point(5, 4);
            btnImport.Margin = new Padding(5, 4, 5, 4);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(305, 68);
            btnImport.TabIndex = 2;
            btnImport.Text = "IMPORT BOOKLIST";
            btnImport.UseVisualStyleBackColor = true;
            btnImport.Click += btnImport_Click;
            // 
            // btnAddBook
            // 
            btnAddBook.Font = new Font("Segoe UI", 16.2F, FontStyle.Bold);
            btnAddBook.Location = new Point(320, 4);
            btnAddBook.Margin = new Padding(5, 4, 5, 4);
            btnAddBook.Name = "btnAddBook";
            btnAddBook.Size = new Size(198, 68);
            btnAddBook.TabIndex = 3;
            btnAddBook.Text = "ADD BOOK";
            btnAddBook.UseVisualStyleBackColor = true;
            btnAddBook.Click += btnAddBook_Click;
            // 
            // btnModifyBook
            // 
            btnModifyBook.Font = new Font("Segoe UI", 16.2F, FontStyle.Bold);
            btnModifyBook.Location = new Point(528, 4);
            btnModifyBook.Margin = new Padding(5, 4, 5, 4);
            btnModifyBook.Name = "btnModifyBook";
            btnModifyBook.Size = new Size(231, 68);
            btnModifyBook.TabIndex = 4;
            btnModifyBook.Text = "MODIFY BOOK";
            btnModifyBook.UseVisualStyleBackColor = true;
            btnModifyBook.Click += btnModifyBook_Click;
            // 
            // btnDeleteBook
            // 
            btnDeleteBook.Font = new Font("Segoe UI", 16.2F, FontStyle.Bold);
            btnDeleteBook.Location = new Point(769, 4);
            btnDeleteBook.Margin = new Padding(5, 4, 5, 4);
            btnDeleteBook.Name = "btnDeleteBook";
            btnDeleteBook.Size = new Size(232, 68);
            btnDeleteBook.TabIndex = 5;
            btnDeleteBook.Text = "DELETE BOOK";
            btnDeleteBook.UseVisualStyleBackColor = true;
            btnDeleteBook.Click += btnDeleteBook_Click;
            // 
            // txtSearch
            // 
            txtSearch.Font = new Font("Microsoft Sans Serif", 25.8000011F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtSearch.Location = new Point(12, 44);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "TYPE HERE TO SEARCH BOOK";
            txtSearch.Size = new Size(1479, 56);
            txtSearch.TabIndex = 8;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 36F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(18, 140);
            label2.Name = "label2";
            label2.Size = new Size(338, 81);
            label2.TabIndex = 9;
            label2.Text = "BOOK LIST";
            // 
            // btnScanBook
            // 
            btnScanBook.Font = new Font("Segoe UI", 16.2F, FontStyle.Bold);
            btnScanBook.Location = new Point(1011, 4);
            btnScanBook.Margin = new Padding(5, 4, 5, 4);
            btnScanBook.Name = "btnScanBook";
            btnScanBook.Size = new Size(247, 68);
            btnScanBook.TabIndex = 10;
            btnScanBook.Text = "SCAN BOOK";
            btnScanBook.UseVisualStyleBackColor = true;
            btnScanBook.Click += btnScanBook_Click;
            // 
            // lblSyncStatus
            // 
            lblSyncStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblSyncStatus.AutoSize = true;
            lblSyncStatus.Font = new Font("Segoe UI", 16.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblSyncStatus.Location = new Point(17, 1014);
            lblSyncStatus.Name = "lblSyncStatus";
            lblSyncStatus.Size = new Size(0, 38);
            lblSyncStatus.TabIndex = 11;
            // 
            // lblNetStatus
            // 
            lblNetStatus.AutoSize = true;
            lblNetStatus.Font = new Font("Segoe UI", 16.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblNetStatus.Location = new Point(17, 8);
            lblNetStatus.Name = "lblNetStatus";
            lblNetStatus.Size = new Size(0, 38);
            lblNetStatus.TabIndex = 13;
            // 
            // flowLayoutPanel_Buttons
            // 
            flowLayoutPanel_Buttons.Anchor = AnchorStyles.Left;
            flowLayoutPanel_Buttons.BackColor = Color.Transparent;
            flowLayoutPanel_Buttons.Controls.Add(btnImport);
            flowLayoutPanel_Buttons.Controls.Add(btnAddBook);
            flowLayoutPanel_Buttons.Controls.Add(btnModifyBook);
            flowLayoutPanel_Buttons.Controls.Add(btnDeleteBook);
            flowLayoutPanel_Buttons.Controls.Add(btnScanBook);
            flowLayoutPanel_Buttons.Controls.Add(btnPricelist);
            flowLayoutPanel_Buttons.Font = new Font("Microsoft Sans Serif", 16.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            flowLayoutPanel_Buttons.Location = new Point(12, 905);
            flowLayoutPanel_Buttons.Name = "flowLayoutPanel_Buttons";
            flowLayoutPanel_Buttons.Size = new Size(1712, 81);
            flowLayoutPanel_Buttons.TabIndex = 14;
            flowLayoutPanel_Buttons.WrapContents = false;
            // 
            // btnPricelist
            // 
            btnPricelist.Font = new Font("Segoe UI", 16.2F, FontStyle.Bold);
            btnPricelist.Location = new Point(1268, 4);
            btnPricelist.Margin = new Padding(5, 4, 5, 4);
            btnPricelist.Name = "btnPricelist";
            btnPricelist.Size = new Size(345, 68);
            btnPricelist.TabIndex = 11;
            btnPricelist.Text = "GENERATE PRICELIST";
            btnPricelist.UseVisualStyleBackColor = true;
            // 
            // lblSuggestion
            // 
            lblSuggestion.AutoSize = true;
            lblSuggestion.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblSuggestion.Location = new Point(18, 82);
            lblSuggestion.Name = "lblSuggestion";
            lblSuggestion.Size = new Size(0, 28);
            lblSuggestion.TabIndex = 15;
            lblSuggestion.TextAlign = ContentAlignment.MiddleLeft;
            lblSuggestion.Click += lblSuggestion_Click;
            // 
            // lblTotalStocks
            // 
            lblTotalStocks.AutoSize = true;
            lblTotalStocks.Font = new Font("Segoe UI", 25.8000011F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTotalStocks.Location = new Point(1186, 158);
            lblTotalStocks.Name = "lblTotalStocks";
            lblTotalStocks.Size = new Size(37, 60);
            lblTotalStocks.TabIndex = 17;
            lblTotalStocks.Text = ".";
            // 
            // ImportBook
            // 
            AutoScaleDimensions = new SizeF(8F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1867, 1053);
            Controls.Add(lblTotalStocks);
            Controls.Add(lblSuggestion);
            Controls.Add(flowLayoutPanel_Buttons);
            Controls.Add(lblNetStatus);
            Controls.Add(lblSyncStatus);
            Controls.Add(label2);
            Controls.Add(txtSearch);
            Controls.Add(btnReload);
            Controls.Add(dgvBookList);
            Font = new Font("Microsoft Sans Serif", 8.25F);
            Name = "ImportBook";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "NCB INVENTORY";
            WindowState = FormWindowState.Maximized;
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
        private TextBox txtSearch;
        private Label label2;
        private Button btnScanBook;
        private Label lblSyncStatus;
        private Label lblNetStatus;
        private FlowLayoutPanel flowLayoutPanel_Buttons;
        private Label lblSuggestion;
        private Label lblTotalStocks;
        private Button btnPricelist;
    }
}