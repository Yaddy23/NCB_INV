namespace NCB_INV
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            textBox1.PasswordChar = '*';
        }

        private void Login_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string correctPassword = "888";

                if (textBox1.Text == correctPassword)
                {
                    lblStatus.Text = "";
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    lblStatus.Text = "❌ Incorrect password. Please try again.";
                    textBox1.Clear();
                    textBox1.Focus();
                }

                e.SuppressKeyPress = true;
            }
        }
    }
}
