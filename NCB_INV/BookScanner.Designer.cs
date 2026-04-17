namespace NCB_INV
{
    partial class BookScanner
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
            btnClose = new Button();
            btnImport = new Button();
            txtBarcodeScanner = new TextBox();
            lblTitle = new Label();
            lblOldQty = new Label();
            lblNewQty = new Label();
            SuspendLayout();
            // 
            // btnClose
            // 
            btnClose.Location = new Point(519, 165);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(134, 62);
            btnClose.TabIndex = 1;
            btnClose.Text = "Close Window";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // btnImport
            // 
            btnImport.Location = new Point(519, 12);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(134, 75);
            btnImport.TabIndex = 2;
            btnImport.Text = "Import Multiple ISBN to add QTY";
            btnImport.UseVisualStyleBackColor = true;
            btnImport.Click += btnImport_Click;
            // 
            // txtBarcodeScanner
            // 
            txtBarcodeScanner.Font = new Font("Segoe UI", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtBarcodeScanner.Location = new Point(12, 12);
            txtBarcodeScanner.Name = "txtBarcodeScanner";
            txtBarcodeScanner.Size = new Size(291, 38);
            txtBarcodeScanner.TabIndex = 3;
            txtBarcodeScanner.KeyDown += txtBarcodeScanner_KeyDown;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblTitle.Location = new Point(12, 71);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(19, 31);
            lblTitle.TabIndex = 6;
            lblTitle.Text = ".";
            // 
            // lblOldQty
            // 
            lblOldQty.AutoSize = true;
            lblOldQty.Font = new Font("Segoe UI", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblOldQty.Location = new Point(12, 119);
            lblOldQty.Name = "lblOldQty";
            lblOldQty.Size = new Size(19, 31);
            lblOldQty.TabIndex = 7;
            lblOldQty.Text = ".";
            // 
            // lblNewQty
            // 
            lblNewQty.AutoSize = true;
            lblNewQty.Font = new Font("Segoe UI", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblNewQty.Location = new Point(12, 165);
            lblNewQty.Name = "lblNewQty";
            lblNewQty.Size = new Size(19, 31);
            lblNewQty.TabIndex = 8;
            lblNewQty.Text = ".";
            // 
            // BookScanner
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(660, 239);
            Controls.Add(lblNewQty);
            Controls.Add(lblOldQty);
            Controls.Add(lblTitle);
            Controls.Add(txtBarcodeScanner);
            Controls.Add(btnImport);
            Controls.Add(btnClose);
            Name = "BookScanner";
            Text = "BookScanner";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button btnClose;
        private Button btnImport;
        private TextBox txtBarcodeScanner;
        private Label lblTitle;
        private Label lblOldQty;
        private Label lblNewQty;
    }
}