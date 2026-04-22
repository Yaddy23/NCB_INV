using static NCB_INV.DBConnection;

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
        }

        private void Login_Load(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string user = txtUsername.Text;
            string pass = txtPassword.Text;

            var account = DBConnection.Login(user, pass);

            if (account != null)
            {
                CurrentSession.User = account;

                string mode = DBConnection. IsCloudAvailable() ? "Online" : "OFFLINE MODE";

                MessageBox.Show($"Welcome, {account.DisplayName}!\nLogged in via: {mode}",
                                "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Hide();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Login Failed! Please check your credentials or network connection.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
