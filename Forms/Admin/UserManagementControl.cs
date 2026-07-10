using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Housing_rental.BLL;
using Housing_rental.Models;

namespace Housing_rental.Forms.Admin
{
    public class UserManagementControl : UserControl
    {
        private readonly UserService _userService;
        private readonly BindingSource _userBindingSource;
        private List<Role> _roles;
        private int _selectedUserId;

        private DataGridView dgvUsers;
        private TextBox txtSearch;
        private TextBox txtFullName;
        private TextBox txtUsername;
        private TextBox txtPhone;
        private TextBox txtEmail;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private ComboBox cmbRole;
        private CheckBox chkIsActive;
        private Button btnNew;
        private Button btnSave;
        private Button btnResetPassword;
        private Button btnToggleActive;
        private Button btnRefresh;
        private Label lblStatus;
        private Label lblMode;

        public UserManagementControl()
        {
            _userService = new UserService();
            _userBindingSource = new BindingSource();
            _roles = new List<Role>();

            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadRoles();
            LoadUsers();
            StartNewUser();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            MinimumSize = new Size(900, 560);
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 9F);

            Panel header = new Panel
            {
                BackColor = Color.White,
                Dock = DockStyle.Top,
                Height = 82
            };

            Label title = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                Location = new Point(24, 16),
                Text = "User Management"
            };

            Label subtitle = new Label
            {
                AutoSize = true,
                ForeColor = Color.DimGray,
                Location = new Point(28, 53),
                Text = "Manage Admin and Staff accounts, roles, active status, and password resets."
            };

            header.Controls.Add(title);
            header.Controls.Add(subtitle);

            Panel leftPanel = new Panel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
                BackColor = Color.White,
                Location = new Point(24, 106),
                Size = new Size(650, 500)
            };

            Label searchLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(18, 18),
                Text = "Search Users"
            };

            txtSearch = new TextBox
            {
                Location = new Point(20, 44),
                Size = new Size(390, 27)
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            btnRefresh = CreateButton("Refresh", 420, 42, 92, 31);
            btnRefresh.Click += BtnRefresh_Click;

            btnNew = CreateButton("New User", 522, 42, 108, 31);
            btnNew.BackColor = Color.FromArgb(31, 90, 160);
            btnNew.ForeColor = Color.White;
            btnNew.FlatStyle = FlatStyle.Flat;
            btnNew.Click += BtnNew_Click;

            dgvUsers = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoGenerateColumns = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                DataSource = _userBindingSource,
                Location = new Point(20, 88),
                MultiSelect = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Size = new Size(610, 388)
            };
            dgvUsers.SelectionChanged += DgvUsers_SelectionChanged;

            AddGridColumn("FullName", "Full Name", 160);
            AddGridColumn("Username", "Username", 100);
            AddGridColumn("RoleName", "Role", 80);
            AddGridColumn("Phone", "Phone", 105);
            AddGridColumn("IsActive", "Active", 60);
            AddGridColumn("LastLoginAt", "Last Login", 120);

            leftPanel.Controls.Add(searchLabel);
            leftPanel.Controls.Add(txtSearch);
            leftPanel.Controls.Add(btnRefresh);
            leftPanel.Controls.Add(btnNew);
            leftPanel.Controls.Add(dgvUsers);

            Panel editorPanel = new Panel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.White,
                Location = new Point(696, 106),
                Size = new Size(376, 500)
            };

            lblMode = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                Location = new Point(22, 18),
                Text = "New User"
            };

            int labelX = 24;
            int inputX = 24;
            int top = 62;
            int inputWidth = 326;

            Label lblFullName = CreateLabel("Full Name", labelX, top);
            txtFullName = CreateTextBox(inputX, top + 22, inputWidth);

            top += 58;
            Label lblUsername = CreateLabel("Username", labelX, top);
            txtUsername = CreateTextBox(inputX, top + 22, inputWidth);

            top += 58;
            Label lblRole = CreateLabel("Role", labelX, top);
            cmbRole = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(inputX, top + 22),
                Size = new Size(inputWidth, 27)
            };

            top += 58;
            Label lblPhone = CreateLabel("Phone", labelX, top);
            txtPhone = CreateTextBox(inputX, top + 22, inputWidth);

            top += 58;
            Label lblEmail = CreateLabel("Email", labelX, top);
            txtEmail = CreateTextBox(inputX, top + 22, inputWidth);

            top += 58;
            chkIsActive = new CheckBox
            {
                AutoSize = true,
                Checked = true,
                Location = new Point(inputX, top + 4),
                Text = "Active user account"
            };

            top += 36;
            Label lblPassword = CreateLabel("Password / New Password", labelX, top);
            txtPassword = CreatePasswordBox(inputX, top + 22, inputWidth);

            top += 58;
            Label lblConfirmPassword = CreateLabel("Confirm Password", labelX, top);
            txtConfirmPassword = CreatePasswordBox(inputX, top + 22, inputWidth);

            btnSave = CreateButton("Save User", 24, 434, 104, 36);
            btnSave.BackColor = Color.FromArgb(31, 90, 160);
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Click += BtnSave_Click;

            btnResetPassword = CreateButton("Reset Password", 136, 434, 124, 36);
            btnResetPassword.Click += BtnResetPassword_Click;

            btnToggleActive = CreateButton("Deactivate", 268, 434, 92, 36);
            btnToggleActive.Click += BtnToggleActive_Click;

            editorPanel.Controls.Add(lblMode);
            editorPanel.Controls.Add(lblFullName);
            editorPanel.Controls.Add(txtFullName);
            editorPanel.Controls.Add(lblUsername);
            editorPanel.Controls.Add(txtUsername);
            editorPanel.Controls.Add(lblRole);
            editorPanel.Controls.Add(cmbRole);
            editorPanel.Controls.Add(lblPhone);
            editorPanel.Controls.Add(txtPhone);
            editorPanel.Controls.Add(lblEmail);
            editorPanel.Controls.Add(txtEmail);
            editorPanel.Controls.Add(chkIsActive);
            editorPanel.Controls.Add(lblPassword);
            editorPanel.Controls.Add(txtPassword);
            editorPanel.Controls.Add(lblConfirmPassword);
            editorPanel.Controls.Add(txtConfirmPassword);
            editorPanel.Controls.Add(btnSave);
            editorPanel.Controls.Add(btnResetPassword);
            editorPanel.Controls.Add(btnToggleActive);

            lblStatus = new Label
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                AutoSize = true,
                ForeColor = Color.DimGray,
                Location = new Point(28, 626),
                Text = "Ready."
            };

            Controls.Add(header);
            Controls.Add(leftPanel);
            Controls.Add(editorPanel);
            Controls.Add(lblStatus);
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
            lblMode.Text = "New User";
            txtFullName.Clear();
            txtUsername.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtPassword.Clear();
            txtConfirmPassword.Clear();
            chkIsActive.Checked = true;
            btnResetPassword.Enabled = false;
            btnToggleActive.Enabled = false;
            btnToggleActive.Text = "Deactivate";

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
            txtFullName.Text = user.FullName;
            txtUsername.Text = user.Username;
            txtPhone.Text = user.Phone;
            txtEmail.Text = user.Email;
            txtPassword.Clear();
            txtConfirmPassword.Clear();
            chkIsActive.Checked = user.IsActive;
            cmbRole.SelectedValue = user.RoleId;
            btnResetPassword.Enabled = true;
            btnToggleActive.Enabled = true;
            btnToggleActive.Text = user.IsActive ? "Deactivate" : "Activate";
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

        private void AddGridColumn(string propertyName, string headerText, int width)
        {
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = propertyName,
                HeaderText = headerText,
                Name = propertyName,
                Width = width
            });
        }

        private Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(x, y),
                Text = text
            };
        }

        private TextBox CreateTextBox(int x, int y, int width)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 27)
            };
        }

        private TextBox CreatePasswordBox(int x, int y, int width)
        {
            TextBox textBox = CreateTextBox(x, y, width);
            textBox.PasswordChar = '*';
            return textBox;
        }

        private Button CreateButton(string text, int x, int y, int width, int height)
        {
            return new Button
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                Text = text,
                UseVisualStyleBackColor = true
            };
        }

        private void SetStatus(string message, bool isError)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = isError ? Color.Firebrick : Color.ForestGreen;
        }
    }
}
