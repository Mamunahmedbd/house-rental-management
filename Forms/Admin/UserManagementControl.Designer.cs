using System;
using System.Drawing;
using System.Windows.Forms;

namespace Housing_rental.Forms.Admin
{
    public partial class UserManagementControl
    {
        private SplitContainer splitContainer;
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
        private Button btnTogglePassword;
        private Button btnToggleConfirmPassword;
        private Label lblStatus;
        private Label lblMode;
        private Label lblPasswordHelp;
        private ToolTip passwordToolTip;
        private const int EditorFieldWidth = 320;

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(248, 250, 252);
            Font = new Font("Segoe UI", 9F);
            passwordToolTip = new ToolTip();

            TableLayoutPanel page = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(28, 24, 28, 18),
                BackColor = Color.FromArgb(248, 250, 252)
            };
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));

            splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 250, 252),
                FixedPanel = FixedPanel.Panel2,
                Panel1MinSize = 120,
                Panel2MinSize = 120,
                SplitterWidth = 18
            };
            splitContainer.Resize += SplitContainer_Resize;

            Panel listPanel = CreateSurfacePanel();
            Panel editorPanel = CreateSurfacePanel();

            BuildUserListPanel(listPanel);
            BuildEditorPanel(editorPanel);

            splitContainer.Panel1.Controls.Add(listPanel);
            splitContainer.Panel2.Controls.Add(editorPanel);

            lblStatus = new Label
            {
                AutoEllipsis = true,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(100, 116, 139),
                Text = "Ready.",
                TextAlign = ContentAlignment.MiddleLeft
            };

            page.Controls.Add(splitContainer, 0, 0);
            page.Controls.Add(lblStatus, 0, 1);
            Controls.Add(page);
        }

        private void BuildUserListPanel(Panel parent)
        {
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(20),
                BackColor = Color.White
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Label heading = new Label
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Text = "System Users",
                TextAlign = ContentAlignment.MiddleLeft
            };

            TableLayoutPanel toolbar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112F));

            txtSearch = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                Margin = new Padding(0, 8, 10, 8),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            btnRefresh = CreateSecondaryButton("Refresh");
            btnRefresh.Margin = new Padding(0, 7, 10, 7);
            btnRefresh.Click += BtnRefresh_Click;

            btnNew = CreatePrimaryButton("New User");
            btnNew.Margin = new Padding(0, 7, 0, 7);
            btnNew.Click += BtnNew_Click;

            toolbar.Controls.Add(txtSearch, 0, 0);
            toolbar.Controls.Add(btnRefresh, 1, 0);
            toolbar.Controls.Add(btnNew, 2, 0);

            dgvUsers = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                DataSource = _userBindingSource,
                Dock = DockStyle.Fill,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(226, 232, 240),
                MultiSelect = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvUsers.SelectionChanged += DgvUsers_SelectionChanged;

            dgvUsers.RowTemplate.Height = 36;
            dgvUsers.ColumnHeadersHeight = 38;
            dgvUsers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(71, 85, 105);
            dgvUsers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvUsers.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(241, 245, 249);
            dgvUsers.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(71, 85, 105);

            dgvUsers.DefaultCellStyle.BackColor = Color.White;
            dgvUsers.DefaultCellStyle.ForeColor = Color.FromArgb(15, 23, 42);
            dgvUsers.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            dgvUsers.DefaultCellStyle.SelectionBackColor = Color.FromArgb(239, 246, 255);
            dgvUsers.DefaultCellStyle.SelectionForeColor = Color.FromArgb(29, 78, 216);

            dgvUsers.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            AddGridColumn("FullName", "Full Name", 28);
            AddGridColumn("Username", "Username", 18);
            AddGridColumn("RoleName", "Role", 14);
            AddGridColumn("Phone", "Phone", 18);
            AddGridColumn("IsActive", "Active", 10);
            AddGridColumn("LastLoginAt", "Last Login", 18);

            layout.Controls.Add(heading, 0, 0);
            layout.Controls.Add(toolbar, 0, 1);
            layout.Controls.Add(dgvUsers, 0, 2);
            parent.Controls.Add(layout);
        }

        private void BuildEditorPanel(Panel parent)
        {
            parent.AutoScroll = true;

            TableLayoutPanel layout = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(22),
                BackColor = Color.White
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            lblMode = new Label
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Text = "New User",
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 0, 16)
            };

            TableLayoutPanel accountFields = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                ColumnCount = 1,
                RowCount = 11,
                Width = EditorFieldWidth
            };

            for (int i = 0; i < 11; i++)
            {
                accountFields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            txtFullName = CreateInput();
            txtUsername = CreateInput();
            cmbRole = new ComboBox
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F),
                Margin = new Padding(0, 0, 0, 12),
                Width = EditorFieldWidth,
                FlatStyle = FlatStyle.Flat
            };
            txtPhone = CreateInput();
            txtEmail = CreateInput();
            chkIsActive = new CheckBox
            {
                AutoSize = true,
                Checked = true,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(51, 65, 85),
                Margin = new Padding(0, 8, 0, 16),
                Text = "Active user account"
            };

            AddField(accountFields, "Full Name", txtFullName);
            AddField(accountFields, "Username", txtUsername);
            AddField(accountFields, "Role", cmbRole);
            AddField(accountFields, "Phone", txtPhone);
            AddField(accountFields, "Email", txtEmail);
            accountFields.Controls.Add(chkIsActive, 0, 10);

            TableLayoutPanel passwordSection = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(0, 8, 0, 0),
                Width = EditorFieldWidth
            };
            for (int i = 0; i < 5; i++)
            {
                passwordSection.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            txtPassword = CreateInput();
            txtPassword.PasswordChar = '*';
            txtConfirmPassword = CreateInput();
            txtConfirmPassword.PasswordChar = '*';
            btnTogglePassword = CreatePasswordToggleButton(txtPassword);
            btnToggleConfirmPassword = CreatePasswordToggleButton(txtConfirmPassword);
            txtPassword.Controls.Add(btnTogglePassword);
            txtConfirmPassword.Controls.Add(btnToggleConfirmPassword);
            btnTogglePassword.Click += BtnTogglePassword_Click;
            btnToggleConfirmPassword.Click += BtnToggleConfirmPassword_Click;

            passwordSection.Controls.Add(CreateFieldLabel("Password"), 0, 0);
            passwordSection.Controls.Add(txtPassword, 0, 1);
            passwordSection.Controls.Add(CreateFieldLabel("Confirm Password"), 0, 2);
            passwordSection.Controls.Add(txtConfirmPassword, 0, 3);

            lblPasswordHelp = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(100, 116, 139),
                MaximumSize = new Size(EditorFieldWidth, 0),
                AutoSize = true,
                Text = "Required when creating a user. For existing users, enter a new password and click Reset Password.",
                TextAlign = ContentAlignment.TopLeft,
                Margin = new Padding(0, 4, 0, 16)
            };
            passwordSection.Controls.Add(lblPasswordHelp, 0, 4);

            TableLayoutPanel actions = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0, 4, 0, 0),
                Width = EditorFieldWidth
            };
            actions.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            actions.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            btnSave = CreatePrimaryButton("Create User");
            btnSave.Dock = DockStyle.Fill;
            btnSave.Click += BtnSave_Click;

            TableLayoutPanel secondaryActions = new TableLayoutPanel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Width = EditorFieldWidth,
                Height = 36,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 8, 0, 0)
            };
            secondaryActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            secondaryActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            btnResetPassword = CreateSecondaryButton("Reset Password");
            btnResetPassword.Dock = DockStyle.Fill;
            btnResetPassword.Margin = new Padding(0, 0, 6, 0);
            btnResetPassword.Click += BtnResetPassword_Click;

            btnToggleActive = CreateSecondaryButton("Deactivate");
            btnToggleActive.Dock = DockStyle.Fill;
            btnToggleActive.Margin = new Padding(6, 0, 0, 0);
            btnToggleActive.Click += BtnToggleActive_Click;

            secondaryActions.Controls.Add(btnResetPassword, 0, 0);
            secondaryActions.Controls.Add(btnToggleActive, 1, 0);

            actions.Controls.Add(btnSave, 0, 0);
            actions.Controls.Add(secondaryActions, 0, 1);

            layout.Controls.Add(lblMode, 0, 0);
            layout.Controls.Add(accountFields, 0, 1);
            layout.Controls.Add(passwordSection, 0, 2);
            layout.Controls.Add(actions, 0, 3);
            parent.Controls.Add(layout);
        }

        private void AddField(TableLayoutPanel layout, string labelText, Control input)
        {
            int row = layout.Controls.Count;
            layout.Controls.Add(CreateFieldLabel(labelText), 0, row);
            layout.Controls.Add(input, 0, row + 1);
        }

        private void SplitContainer_Resize(object sender, EventArgs e)
        {
            AdjustSplitter();
        }

        private void AdjustSplitter()
        {
            int totalWidth = splitContainer.Width;
            int minimumRequired = splitContainer.Panel1MinSize + splitContainer.Panel2MinSize + splitContainer.SplitterWidth;

            if (totalWidth <= minimumRequired)
            {
                return;
            }

            int editorWidth = totalWidth >= 900
                ? Math.Min(480, Math.Max(380, totalWidth / 3))
                : Math.Max(260, totalWidth / 3);

            int distance = totalWidth - splitContainer.SplitterWidth - editorWidth;
            int minDistance = splitContainer.Panel1MinSize;
            int maxDistance = totalWidth - splitContainer.SplitterWidth - splitContainer.Panel2MinSize;

            if (maxDistance < minDistance)
            {
                return;
            }

            splitContainer.SplitterDistance = Math.Max(minDistance, Math.Min(distance, maxDistance));
        }

        private void AddGridColumn(string propertyName, string headerText, float fillWeight)
        {
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = propertyName,
                FillWeight = fillWeight,
                HeaderText = headerText,
                Name = propertyName
            });
        }

        private Panel CreateSurfacePanel()
        {
            Panel panel = new Panel
            {
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };
            panel.Paint += Panel_Paint;
            return panel;
        }

        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, ((Panel)sender).ClientRectangle,
                Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
        }

        private Label CreateFieldLabel(string text)
        {
            return new Label
            {
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                Margin = new Padding(0, 4, 0, 4),
                Text = text,
                TextAlign = ContentAlignment.BottomLeft,
                Width = EditorFieldWidth
            };
        }

        private TextBox CreateInput()
        {
            return new TextBox
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Font = new Font("Segoe UI", 9.5F),
                Margin = new Padding(0, 0, 0, 12),
                Width = EditorFieldWidth,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private Button CreatePasswordToggleButton(TextBox parentTextBox)
        {
            Button button = new Button
            {
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false,
                Size = new Size(24, 18),
                Location = new Point(parentTextBox.Width - 28, 2),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(226, 232, 240);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(241, 245, 249);
            passwordToolTip.SetToolTip(button, "Show password");
            return button;
        }

        private Button CreatePrimaryButton(string text)
        {
            Button button = CreateBaseButton(text);
            button.BackColor = Color.FromArgb(37, 99, 235);
            button.ForeColor = Color.White;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(29, 78, 216);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 64, 175);
            return button;
        }

        private Button CreateSecondaryButton(string text)
        {
            Button button = CreateBaseButton(text);
            button.BackColor = Color.FromArgb(241, 245, 249);
            button.ForeColor = Color.FromArgb(51, 65, 85);
            button.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(203, 213, 225);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(226, 232, 240);
            return button;
        }

        private Button CreateBaseButton(string text)
        {
            Button button = new Button
            {
                Height = 36,
                Text = text,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                UseVisualStyleBackColor = false,
                Width = 92
            };
            button.FlatAppearance.BorderSize = 1;
            return button;
        }
    }
}
