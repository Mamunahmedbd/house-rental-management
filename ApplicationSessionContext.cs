using System;
using System.Windows.Forms;
using Housing_rental.BLL;
using Housing_rental.Forms.Auth;
using Housing_rental.Forms.Dashboard;

namespace Housing_rental
{
    internal sealed class ApplicationSessionContext : ApplicationContext
    {
        private FrmLogin _loginForm;
        private FrmDashboard _dashboardForm;
        private bool _returnToLogin;

        public ApplicationSessionContext()
        {
            ShowLogin();
        }

        private void ShowLogin()
        {
            CurrentSession.End();

            _loginForm = new FrmLogin();
            _loginForm.LoginSucceeded += LoginForm_LoginSucceeded;
            _loginForm.FormClosed += LoginForm_FormClosed;
            _loginForm.Show();
        }

        private void LoginForm_LoginSucceeded(object sender, EventArgs e)
        {
            _loginForm.LoginSucceeded -= LoginForm_LoginSucceeded;
            _loginForm.FormClosed -= LoginForm_FormClosed;
            _loginForm.Close();
            _loginForm.Dispose();
            _loginForm = null;

            ShowDashboard();
        }

        private void LoginForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            CurrentSession.End();
            ExitThread();
        }

        private void ShowDashboard()
        {
            _dashboardForm = new FrmDashboard();
            _dashboardForm.LogoutRequested += DashboardForm_LogoutRequested;
            _dashboardForm.FormClosed += DashboardForm_FormClosed;
            _dashboardForm.Show();
        }

        private void DashboardForm_LogoutRequested(object sender, EventArgs e)
        {
            _returnToLogin = true;
            _dashboardForm.Close();
        }

        private void DashboardForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _dashboardForm.LogoutRequested -= DashboardForm_LogoutRequested;
            _dashboardForm.FormClosed -= DashboardForm_FormClosed;
            _dashboardForm.Dispose();
            _dashboardForm = null;

            CurrentSession.End();

            if (_returnToLogin)
            {
                _returnToLogin = false;
                ShowLogin();
                return;
            }

            ExitThread();
        }
    }
}
