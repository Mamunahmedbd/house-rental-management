using System;
using System.Windows.Forms;
using Housing_rental.BLL;
using Housing_rental.Forms.Dashboard;
using Housing_rental.Models;

namespace Housing_rental.Forms.Auth
{
    public partial class FrmLogin : Form
    {
        private readonly AuthService _authService;

        public FrmLogin()
        {
            InitializeComponent();
            _authService = new AuthService();
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            ServiceResult<User> result = _authService.Login(txtUsername.Text, txtPassword.Text);

            if (!result.IsSuccess)
            {
                MessageBox.Show(result.Message, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Hide();

            using (FrmDashboard dashboard = new FrmDashboard())
            {
                dashboard.ShowDialog();
            }

            CurrentSession.End();
            txtPassword.Clear();
            Show();
            txtUsername.Focus();
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
