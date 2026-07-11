using System;
using System.Drawing;
using System.Windows.Forms;

namespace Housing_rental.Forms.Settings
{
    public partial class SettingsManagementControl
    {
        private TabControl tabControl;
        private TabPage tabSettings;
        private TabPage tabProfile;

        // Tab 1: Application Settings controls
        private TextBox txtApplicationName;
        private TextBox txtDefaultCurrency;
        private NumericUpDown nudRentDueDay;
        private TextBox txtReceiptFooter;
        private Button btnSaveSettings;
        private Label lblSettingsStatus;

        // Tab 2: My Profile controls
        private Label lblFullNameValue;
        private Label lblUsernameValue;
        private Label lblRoleValue;
        private Label lblEmailValue;
        private Label lblLastLoginValue;
        private TextBox txtCurrentPassword;
        private TextBox txtNewPassword;
        private TextBox txtConfirmNewPassword;
        private Button btnChangePassword;
        private Label lblProfileStatus;

        private const int EditorFieldWidth = 400;

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(248, 250, 252);
            Font = new Font("Segoe UI", 9F);

            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 1,
                Padding = new Padding(28, 24, 28, 18),
                BackColor = Color.FromArgb(248, 250, 252)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                Padding = new Point(12, 6)
            };

            tabSettings = new TabPage
            {
                Text = "  Application Settings  ",
                BackColor = Color.White,
                Padding = new Padding(24)
            };

            tabProfile = new TabPage
            {
                Text = "  My Profile  ",
                BackColor = Color.White,
                Padding = new Padding(24)
            };

            BuildSettingsTab();
            BuildProfileTab();

            tabControl.TabPages.Add(tabSettings);
            tabControl.TabPages.Add(tabProfile);

            mainLayout.Controls.Add(tabControl, 0, 0);
            Controls.Add(mainLayout);
        }

        private void BuildSettingsTab()
        {
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = Color.White
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F)); // General
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 160F)); // Financial
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140F)); // Receipt
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));  // Buttons & Status
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // Spacer

            // 1. General Settings Group
            GroupBox grpGeneral = new GroupBox
            {
                Text = "General Settings",
                Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 16)
            };
            FlowLayoutPanel flowGeneral = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(12)
            };
            flowGeneral.Controls.Add(CreateFieldLabel("Application Name"));
            txtApplicationName = CreateInput();
            flowGeneral.Controls.Add(txtApplicationName);
            grpGeneral.Controls.Add(flowGeneral);

            // 2. Financial Settings Group
            GroupBox grpFinancial = new GroupBox
            {
                Text = "Financial Settings",
                Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 16)
            };
            FlowLayoutPanel flowFinancial = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(12)
            };
            
            flowFinancial.Controls.Add(CreateFieldLabel("Default Currency Code"));
            txtDefaultCurrency = CreateInput();
            txtDefaultCurrency.MaxLength = 3;
            txtDefaultCurrency.CharacterCasing = CharacterCasing.Upper;
            flowFinancial.Controls.Add(txtDefaultCurrency);

            flowFinancial.Controls.Add(CreateFieldLabel("Rent Due Day (1-28)"));
            nudRentDueDay = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 28,
                Value = 5,
                Width = 100,
                Font = new Font("Segoe UI", 9.5F),
                Margin = new Padding(0, 0, 0, 12)
            };
            flowFinancial.Controls.Add(nudRentDueDay);
            grpFinancial.Controls.Add(flowFinancial);

            // 3. Receipt Configuration Group
            GroupBox grpReceipt = new GroupBox
            {
                Text = "Receipt Configuration",
                Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 16)
            };
            FlowLayoutPanel flowReceipt = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(12)
            };
            flowReceipt.Controls.Add(CreateFieldLabel("Receipt Footer Text"));
            txtReceiptFooter = CreateInput();
            txtReceiptFooter.Multiline = true;
            txtReceiptFooter.Height = 60;
            txtReceiptFooter.MaxLength = 300;
            flowReceipt.Controls.Add(txtReceiptFooter);
            grpReceipt.Controls.Add(flowReceipt);

            // 4. Action Panel
            FlowLayoutPanel actionPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0)
            };
            btnSaveSettings = CreatePrimaryButton("Save Settings");
            btnSaveSettings.Click += BtnSaveSettings_Click;
            btnSaveSettings.Width = 140;

            lblSettingsStatus = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                Margin = new Padding(12, 8, 0, 0)
            };

            actionPanel.Controls.Add(btnSaveSettings);
            actionPanel.Controls.Add(lblSettingsStatus);

            layout.Controls.Add(grpGeneral, 0, 0);
            layout.Controls.Add(grpFinancial, 0, 1);
            layout.Controls.Add(grpReceipt, 0, 2);
            layout.Controls.Add(actionPanel, 0, 3);

            tabSettings.Controls.Add(layout);
        }

        private void BuildProfileTab()
        {
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Color.White,
                Padding = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 340F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // 1. User Information Panel (Left Column)
            GroupBox grpUserInfo = new GroupBox
            {
                Text = "My Profile Information",
                Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 12, 0)
            };
            TableLayoutPanel infoTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(16)
            };
            infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            string[] labels = { "Full Name:", "Username:", "Role:", "Email:", "Last Login:" };
            Label[] valueLabels = {
                lblFullNameValue = CreateValueLabel(),
                lblUsernameValue = CreateValueLabel(),
                lblRoleValue = CreateValueLabel(),
                lblEmailValue = CreateValueLabel(),
                lblLastLoginValue = CreateValueLabel()
            };

            for (int i = 0; i < labels.Length; i++)
            {
                infoTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
                infoTable.Controls.Add(CreateProfileLabel(labels[i]), 0, i);
                infoTable.Controls.Add(valueLabels[i], 1, i);
            }
            grpUserInfo.Controls.Add(infoTable);

            // 2. Change Password Panel (Right Column)
            GroupBox grpChangePassword = new GroupBox
            {
                Text = "Change Password",
                Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Dock = DockStyle.Fill,
                Margin = new Padding(12, 0, 0, 0)
            };
            FlowLayoutPanel flowPassword = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(16)
            };

            flowPassword.Controls.Add(CreateFieldLabel("Current Password"));
            txtCurrentPassword = CreateInput();
            txtCurrentPassword.PasswordChar = '●';
            flowPassword.Controls.Add(txtCurrentPassword);

            flowPassword.Controls.Add(CreateFieldLabel("New Password"));
            txtNewPassword = CreateInput();
            txtNewPassword.PasswordChar = '●';
            flowPassword.Controls.Add(txtNewPassword);

            flowPassword.Controls.Add(CreateFieldLabel("Confirm New Password"));
            txtConfirmNewPassword = CreateInput();
            txtConfirmNewPassword.PasswordChar = '●';
            flowPassword.Controls.Add(txtConfirmNewPassword);

            btnChangePassword = CreatePrimaryButton("Change Password");
            btnChangePassword.Click += BtnChangePassword_Click;
            btnChangePassword.Width = 140;
            flowPassword.Controls.Add(btnChangePassword);

            lblProfileStatus = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                Margin = new Padding(0, 8, 0, 0)
            };
            flowPassword.Controls.Add(lblProfileStatus);

            grpChangePassword.Controls.Add(flowPassword);

            layout.Controls.Add(grpUserInfo, 0, 0);
            layout.Controls.Add(grpChangePassword, 1, 0);

            tabProfile.Controls.Add(layout);
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

        private Label CreateProfileLabel(string text)
        {
            return new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                TextAlign = ContentAlignment.MiddleLeft,
                Height = 35
            };
        }

        private Label CreateValueLabel()
        {
            return new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(15, 23, 42),
                TextAlign = ContentAlignment.MiddleLeft,
                Height = 35
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
