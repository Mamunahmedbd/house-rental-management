using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Housing_rental.BLL;
using Housing_rental.Models;

namespace Housing_rental.Forms.Admin
{
    public partial class UserManagementControl : UserControl
    {
        private readonly UserService _userService;
        private readonly BindingSource _userBindingSource;
        private List<Role> _roles;
        private int _selectedUserId;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private const int EM_SETMARGINS = 0xd3;
        private const int EC_RIGHTMARGIN = 2;

        public UserManagementControl()
        {
            _userService = new UserService();
            _userBindingSource = new BindingSource();
            _roles = new List<Role>();

            InitializeComponent();

            btnTogglePassword.Paint += BtnTogglePassword_Paint;
            btnToggleConfirmPassword.Paint += BtnTogglePassword_Paint;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadRoles();
            LoadUsers();
            StartNewUser();
            AdjustSplitter();
            SetPasswordTextBoxMargins();
        }

        private void SetPasswordTextBoxMargins()
        {
            try
            {
                // Set right margin of password textboxes to leave space for the inline toggle buttons
                int marginWidth = btnTogglePassword.Width + 8; // 24 + 8 = 32 pixels
                SendMessage(txtPassword.Handle, EM_SETMARGINS, EC_RIGHTMARGIN, marginWidth << 16);
                SendMessage(txtConfirmPassword.Handle, EM_SETMARGINS, EC_RIGHTMARGIN, marginWidth << 16);
            }
            catch (Exception)
            {
                // Fallback in case handle is not created or API call fails
            }
        }

        private void LoadRoles()
        {
            ServiceResult<List<Role>> result = _userService.GetActiveRoles();

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                return;
            }

            _roles = result.Data;
            cmbRole.DataSource = null;
            cmbRole.DataSource = _roles;
            cmbRole.DisplayMember = "RoleName";
            cmbRole.ValueMember = "RoleId";
        }

        private void LoadUsers()
        {
            ServiceResult<List<User>> result = _userService.SearchUsers(txtSearch == null ? string.Empty : txtSearch.Text);

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                return;
            }

            _userBindingSource.DataSource = result.Data;
            SetStatus(result.Message, false);
        }

        private void StartNewUser()
        {
            _selectedUserId = 0;
            lblMode.Text = "Create User";
            btnSave.Text = "Create User";
            txtFullName.Clear();
            txtUsername.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtPassword.Clear();
            txtConfirmPassword.Clear();
            SetPasswordVisibility(false);
            chkIsActive.Checked = true;
            btnResetPassword.Enabled = false;
            btnToggleActive.Enabled = false;
            btnToggleActive.Text = "Deactivate";
            lblPasswordHelp.Text = "Password and confirm password are required when creating a new user.";

            if (cmbRole.Items.Count > 0)
            {
                cmbRole.SelectedIndex = 0;
            }

            txtFullName.Focus();
        }

        private void LoadSelectedUser(User user)
        {
            if (user == null)
            {
                return;
            }

            _selectedUserId = user.UserId;
            lblMode.Text = "Edit User";
            btnSave.Text = "Save Changes";
            txtFullName.Text = user.FullName;
            txtUsername.Text = user.Username;
            txtPhone.Text = user.Phone;
            txtEmail.Text = user.Email;
            txtPassword.Clear();
            txtConfirmPassword.Clear();
            SetPasswordVisibility(false);
            chkIsActive.Checked = user.IsActive;
            cmbRole.SelectedValue = user.RoleId;
            btnResetPassword.Enabled = true;
            btnToggleActive.Enabled = true;
            btnToggleActive.Text = user.IsActive ? "Deactivate" : "Activate";
            lblPasswordHelp.Text = "Leave password fields empty unless you want to reset this user's password.";
        }

        private User ReadUserFromForm()
        {
            Role selectedRole = cmbRole.SelectedItem as Role;

            return new User
            {
                UserId = _selectedUserId,
                RoleId = selectedRole == null ? 0 : selectedRole.RoleId,
                FullName = txtFullName.Text,
                Username = txtUsername.Text,
                Phone = txtPhone.Text,
                Email = txtEmail.Text,
                IsActive = chkIsActive.Checked
            };
        }

        private void SaveUser()
        {
            User user = ReadUserFromForm();
            ServiceResult result = _selectedUserId == 0
                ? _userService.CreateUser(user, txtPassword.Text, txtConfirmPassword.Text)
                : _userService.UpdateUser(user);

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                MessageBox.Show(result.Message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetStatus(result.Message, false);
            LoadUsers();
            SelectUserById(_selectedUserId == 0 ? FindUserIdByUsername(user.Username) : user.UserId);
            txtPassword.Clear();
            txtConfirmPassword.Clear();
        }

        private int FindUserIdByUsername(string username)
        {
            foreach (User user in _userBindingSource.List)
            {
                if (string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase))
                {
                    return user.UserId;
                }
            }

            return 0;
        }

        private void SelectUserById(int userId)
        {
            if (userId <= 0)
            {
                return;
            }

            foreach (DataGridViewRow row in dgvUsers.Rows)
            {
                User user = row.DataBoundItem as User;

                if (user != null && user.UserId == userId)
                {
                    row.Selected = true;
                    dgvUsers.CurrentCell = row.Cells[0];
                    LoadSelectedUser(user);
                    return;
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveUser();
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            StartNewUser();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadUsers();
        }

        private void BtnResetPassword_Click(object sender, EventArgs e)
        {
            if (_selectedUserId <= 0)
            {
                SetStatus("Please select a user first.", true);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Reset password for the selected user?",
                "Confirm Password Reset",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            ServiceResult result = _userService.ResetPassword(_selectedUserId, txtPassword.Text, txtConfirmPassword.Text);

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                MessageBox.Show(result.Message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            txtPassword.Clear();
            txtConfirmPassword.Clear();
            SetPasswordVisibility(false);
            SetStatus(result.Message, false);
        }

        private void BtnToggleActive_Click(object sender, EventArgs e)
        {
            User selectedUser = _userBindingSource.Current as User;

            if (selectedUser == null)
            {
                SetStatus("Please select a user first.", true);
                return;
            }

            bool newStatus = !selectedUser.IsActive;
            string action = newStatus ? "activate" : "deactivate";

            DialogResult confirm = MessageBox.Show(
                "Are you sure you want to " + action + " this user?",
                "Confirm Status Change",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            ServiceResult result = _userService.SetActiveStatus(selectedUser.UserId, newStatus);

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                MessageBox.Show(result.Message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetStatus(result.Message, false);
            LoadUsers();
            SelectUserById(selectedUser.UserId);
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadUsers();
        }

        private void DgvUsers_SelectionChanged(object sender, EventArgs e)
        {
            LoadSelectedUser(_userBindingSource.Current as User);
        }

        private void BtnTogglePassword_Click(object sender, EventArgs e)
        {
            TogglePasswordVisibility(txtPassword, btnTogglePassword);
        }

        private void BtnToggleConfirmPassword_Click(object sender, EventArgs e)
        {
            TogglePasswordVisibility(txtConfirmPassword, btnToggleConfirmPassword);
        }

        private void SetPasswordVisibility(bool visible)
        {
            txtPassword.PasswordChar = visible ? '\0' : '*';
            txtConfirmPassword.PasswordChar = visible ? '\0' : '*';
            passwordToolTip.SetToolTip(btnTogglePassword, visible ? "Hide password" : "Show password");
            passwordToolTip.SetToolTip(btnToggleConfirmPassword, visible ? "Hide confirm password" : "Show confirm password");
            btnTogglePassword.Invalidate();
            btnToggleConfirmPassword.Invalidate();
        }

        private void TogglePasswordVisibility(TextBox textBox, Button button)
        {
            bool willShow = textBox.PasswordChar != '\0';
            textBox.PasswordChar = willShow ? '\0' : '*';
            passwordToolTip.SetToolTip(button, willShow ? "Hide password" : "Show password");
            button.Invalidate();
        }

        private void BtnTogglePassword_Paint(object sender, PaintEventArgs e)
        {
            Button button = sender as Button;
            if (button == null) return;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int w = button.Width;
            int h = button.Height;

            // We draw on a 24x24 virtual grid, scaled to fit the button
            float scale = Math.Min(w, h) / 24f;
            e.Graphics.ScaleTransform(scale, scale);

            // Shift to center the drawing inside the button
            float offsetX = (w / scale - 24f) / 2f;
            float offsetY = (h / scale - 24f) / 2f;
            e.Graphics.TranslateTransform(offsetX, offsetY);

            // Determine if the associated textbox is masked
            TextBox assocTextBox = (button == btnTogglePassword) ? txtPassword : txtConfirmPassword;
            bool isMasked = assocTextBox.PasswordChar != '\0';

            // Sleek slate-500 color (#64748B)
            Color iconColor = Color.FromArgb(100, 116, 139);
            using (Pen pen = new Pen(iconColor, 1.8f))
            {
                pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

                if (isMasked)
                {
                    // Eye-off state (hidden password) - Lucide eye-off design
                    // 1. Diagonal slash line from (3, 3) to (21, 21)
                    e.Graphics.DrawLine(pen, 3f, 3f, 21f, 21f);

                    // 2. Eyelids
                    e.Graphics.DrawBezier(pen, 
                        new PointF(2, 12), 
                        new PointF(6, 5), 
                        new PointF(18, 5), 
                        new PointF(22, 12));
                    
                    e.Graphics.DrawBezier(pen, 
                        new PointF(2, 12), 
                        new PointF(6, 19), 
                        new PointF(18, 19), 
                        new PointF(22, 12));

                    // 3. Iris outline
                    e.Graphics.DrawEllipse(pen, 9f, 9f, 6f, 6f);
                }
                else
                {
                    // Eye state (visible password) - Lucide eye design
                    // 1. Eyelids
                    e.Graphics.DrawBezier(pen, 
                        new PointF(2, 12), 
                        new PointF(6, 5), 
                        new PointF(18, 5), 
                        new PointF(22, 12));
                    
                    e.Graphics.DrawBezier(pen, 
                        new PointF(2, 12), 
                        new PointF(6, 19), 
                        new PointF(18, 19), 
                        new PointF(22, 12));

                    // 2. Iris outline
                    e.Graphics.DrawEllipse(pen, 9f, 9f, 6f, 6f);

                    // 3. Pupil filled dot
                    using (SolidBrush brush = new SolidBrush(iconColor))
                    {
                        e.Graphics.FillEllipse(brush, 11f, 11f, 2f, 2f);
                    }
                }
            }
        }

        private void SetStatus(string message, bool isError)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = isError ? Color.FromArgb(239, 68, 68) : Color.FromArgb(16, 185, 129);
        }
    }
}
