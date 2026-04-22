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
                MessageBox.Show($"Welcome, {CurrentSession.User.DisplayName}!");
                this.Hide();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                string debugHash = HashPassword(pass);
                MessageBox.Show($"Login Failed!\n\nTyped User: {user}\nTyped Pass: {pass}\nGenerated Hash: {debugHash}");
            }
        }
    }
}
