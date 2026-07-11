using System.Drawing;
using System.Windows.Forms;

namespace Housing_rental.Forms.Tenants
{
    public partial class TenantManagementControl
    {
        private SplitContainer splitContainer;
        private TableLayoutPanel pageLayout;
        private Panel toolbarPanel;
        private TextBox txtSearch;
        private ComboBox cmbFilterStatus;
        private CheckBox chkIncludeInactive;
        private Button btnRefresh;
        private Button btnNew;
        private TabControl tabMain;
        private TabPage tabTenants;
        private TabPage tabDetails;
        private TabPage tabAgreements;
        private TabPage tabPayments;
        private DataGridView dgvTenants;
        private DataGridView dgvCurrentOccupancy;
        private DataGridView dgvBalance;
        private DataGridView dgvAgreements;
        private DataGridView dgvPayments;
        private Label lblStatus;
        private Label lblMode;
        private TextBox txtFullName;
        private TextBox txtPhone;
        private TextBox txtEmail;
        private TextBox txtNationalId;
        private TextBox txtAddress;
        private TextBox txtEmergencyContactName;
        private TextBox txtEmergencyContactPhone;
        private ComboBox cmbTenantStatus;
        private Button btnSave;
        private Button btnToggleStatus;
        private Button btnBlacklist;
        private const int EditorWidth = 330;

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(248, 250, 252);
            Font = new Font("Segoe UI", 9F);

            pageLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(24, 20, 24, 16),
                BackColor = Color.FromArgb(248, 250, 252)
            };
            pageLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            pageLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            pageLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));

            BuildToolbar();
            BuildTabs();

            lblStatus = new Label
            {
                AutoEllipsis = true,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(100, 116, 139),
                Text = "Ready.",
                TextAlign = ContentAlignment.MiddleLeft
            };

            pageLayout.Controls.Add(toolbarPanel, 0, 0);
            pageLayout.Controls.Add(tabMain, 0, 1);
            pageLayout.Controls.Add(lblStatus, 0, 2);
            Controls.Add(pageLayout);
        }

        private void BuildToolbar()
        {
            toolbarPanel = CreateSurfacePanel();
            toolbarPanel.Margin = new Padding(0, 0, 0, 12);
            toolbarPanel.Padding = new Padding(16, 2, 16, 2);

            TableLayoutPanel toolbar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 1,
                BackColor = Color.White
            };
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108F));

            txtSearch = CreateInput();
            txtSearch.Dock = DockStyle.Fill;
            txtSearch.Margin = new Padding(0, 8, 10, 8);
            txtSearch.TextChanged += TxtSearch_TextChanged;

            cmbFilterStatus = CreateCombo();
            cmbFilterStatus.Dock = DockStyle.Fill;
            cmbFilterStatus.Margin = new Padding(0, 8, 10, 8);
            cmbFilterStatus.Items.AddRange(new object[] { "All statuses", "Active", "Inactive", "Blacklisted" });
            cmbFilterStatus.SelectedIndexChanged += CmbFilterStatus_SelectedIndexChanged;

            chkIncludeInactive = new CheckBox
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(71, 85, 105),
                Text = "Inactive",
                TextAlign = ContentAlignment.MiddleLeft
            };
            chkIncludeInactive.CheckedChanged += ChkIncludeInactive_CheckedChanged;

            btnRefresh = CreateSecondaryButton("Refresh");
            btnRefresh.Dock = DockStyle.Fill;
            btnRefresh.Margin = new Padding(0, 8, 10, 8);
            btnRefresh.Click += BtnRefresh_Click;

            btnNew = CreatePrimaryButton("New Tenant");
            btnNew.Dock = DockStyle.Fill;
            btnNew.Margin = new Padding(0, 8, 0, 8);
            btnNew.Click += BtnNew_Click;

            toolbar.Controls.Add(txtSearch, 0, 0);
            toolbar.Controls.Add(cmbFilterStatus, 1, 0);
            toolbar.Controls.Add(chkIncludeInactive, 2, 0);
            toolbar.Controls.Add(btnRefresh, 3, 0);
            toolbar.Controls.Add(btnNew, 4, 0);
            toolbarPanel.Controls.Add(toolbar);
        }

        private void BuildTabs()
        {
            tabMain = new TabControl
            {
                Dock = DockStyle.Fill,
                DrawMode = TabDrawMode.OwnerDrawFixed,
                Font = new Font("Segoe UI", 9.5F),
                ItemSize = new Size(150, 42),
                SizeMode = TabSizeMode.Fixed
            };
            tabMain.DrawItem += TabMain_DrawItem;
            tabMain.SelectedIndexChanged += TabMain_SelectedIndexChanged;

            tabTenants = new TabPage("Tenants") { BackColor = Color.FromArgb(248, 250, 252) };
            tabDetails = new TabPage("Details") { BackColor = Color.FromArgb(248, 250, 252) };
            tabAgreements = new TabPage("Agreement History") { BackColor = Color.FromArgb(248, 250, 252) };
            tabPayments = new TabPage("Payment History") { BackColor = Color.FromArgb(248, 250, 252) };

            BuildTenantsTab();
            BuildDetailsTab();
            BuildAgreementsTab();
            BuildPaymentsTab();

            tabMain.TabPages.Add(tabTenants);
            tabMain.TabPages.Add(tabDetails);
            tabMain.TabPages.Add(tabAgreements);
            tabMain.TabPages.Add(tabPayments);
        }

        private void BuildTenantsTab()
        {
            splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 250, 252),
                FixedPanel = FixedPanel.Panel2,
                Panel1MinSize = 120,
                Panel2MinSize = 120,
                SplitterWidth = 16
            };
            splitContainer.Resize += SplitContainer_Resize;

            Panel listPanel = CreateSurfacePanel();
            listPanel.Padding = new Padding(0);
            dgvTenants = CreateGrid(_tenantBindingSource);
            dgvTenants.SelectionChanged += DgvTenants_SelectionChanged;
            AddGridColumn(dgvTenants, "FullName", "Tenant", 24);
            AddGridColumn(dgvTenants, "Phone", "Phone", 16);
            AddGridColumn(dgvTenants, "Email", "Email", 20);
            AddGridColumn(dgvTenants, "NationalId", "National ID", 16);
            AddGridColumn(dgvTenants, "Status", "Status", 12);
            AddGridColumn(dgvTenants, "CreatedAt", "Created", 12);
            listPanel.Controls.Add(dgvTenants);

            Panel editorPanel = CreateSurfacePanel();
            editorPanel.AutoScroll = true;
            BuildEditorPanel(editorPanel);

            splitContainer.Panel1.Controls.Add(listPanel);
            splitContainer.Panel2.Controls.Add(editorPanel);
            tabTenants.Controls.Add(splitContainer);
        }

        private void BuildEditorPanel(Panel parent)
        {
            TableLayoutPanel editor = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 19,
                Padding = new Padding(22),
                BackColor = Color.White
            };

            for (int i = 0; i < 19; i++)
            {
                editor.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            lblMode = CreateEditorHeading("Create Tenant");
            txtFullName = CreateInput();
            txtPhone = CreateInput();
            txtEmail = CreateInput();
            txtNationalId = CreateInput();
            txtAddress = CreateMultilineInput();
            txtEmergencyContactName = CreateInput();
            txtEmergencyContactPhone = CreateInput();
            cmbTenantStatus = CreateCombo();
            cmbTenantStatus.Items.AddRange(new object[] { "Active", "Inactive", "Blacklisted" });

            btnSave = CreatePrimaryButton("Create Tenant");
            btnSave.Dock = DockStyle.Fill;
            btnSave.Click += BtnSave_Click;

            btnToggleStatus = CreateSecondaryButton("Deactivate");
            btnToggleStatus.Dock = DockStyle.Fill;
            btnToggleStatus.Click += BtnToggleStatus_Click;

            btnBlacklist = CreateSecondaryButton("Blacklist");
            btnBlacklist.Dock = DockStyle.Fill;
            btnBlacklist.Click += BtnBlacklist_Click;

            AddHeading(editor, lblMode);
            AddField(editor, "Full Name", txtFullName);
            AddField(editor, "Phone", txtPhone);
            AddField(editor, "Email", txtEmail);
            AddField(editor, "National ID", txtNationalId);
            AddField(editor, "Address", txtAddress);
            AddField(editor, "Emergency Contact Name", txtEmergencyContactName);
            AddField(editor, "Emergency Contact Phone", txtEmergencyContactPhone);
            AddField(editor, "Status", cmbTenantStatus);
            editor.Controls.Add(CreateActionRow(), 0, editor.Controls.Count);

            parent.Controls.Add(editor);
        }

        private void BuildDetailsTab()
        {
            TableLayoutPanel detailsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0),
                BackColor = Color.FromArgb(248, 250, 252)
            };
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));

            Panel occupancyPanel = CreateSurfacePanel();
            occupancyPanel.Padding = new Padding(0);
            occupancyPanel.Margin = new Padding(0, 0, 0, 8);
            dgvCurrentOccupancy = CreateGrid(_currentOccupancyBindingSource);
            AddGridColumn(dgvCurrentOccupancy, "AgreementNo", "Agreement", 14);
            AddGridColumn(dgvCurrentOccupancy, "PropertyName", "Property", 18);
            AddGridColumn(dgvCurrentOccupancy, "HouseName", "House", 14);
            AddGridColumn(dgvCurrentOccupancy, "RoomNo", "Room", 10);
            AddGridColumn(dgvCurrentOccupancy, "RoomType", "Type", 10);
            AddGridColumn(dgvCurrentOccupancy, "StartDate", "Start", 12);
            AddGridColumn(dgvCurrentOccupancy, "EndDate", "End", 12);
            AddGridColumn(dgvCurrentOccupancy, "MonthlyRent", "Rent", 10);
            AddGridColumn(dgvCurrentOccupancy, "AgreementStatus", "Agreement Status", 14);
            occupancyPanel.Controls.Add(dgvCurrentOccupancy);

            Panel balancePanel = CreateSurfacePanel();
            balancePanel.Padding = new Padding(0);
            balancePanel.Margin = new Padding(0, 8, 0, 0);
            dgvBalance = CreateGrid(_balanceBindingSource);
            AddGridColumn(dgvBalance, "FullName", "Tenant", 22);
            AddGridColumn(dgvBalance, "TotalDue", "Total Due", 16);
            AddGridColumn(dgvBalance, "TotalPaid", "Total Paid", 16);
            AddGridColumn(dgvBalance, "TotalBalance", "Balance", 16);
            AddGridColumn(dgvBalance, "PaymentCount", "Payments", 12);
            AddGridColumn(dgvBalance, "OverdueCount", "Overdue", 12);
            balancePanel.Controls.Add(dgvBalance);

            detailsLayout.Controls.Add(occupancyPanel, 0, 0);
            detailsLayout.Controls.Add(balancePanel, 0, 1);
            tabDetails.Controls.Add(detailsLayout);
        }

        private void BuildAgreementsTab()
        {
            Panel surface = CreateSurfacePanel();
            surface.Padding = new Padding(0);
            dgvAgreements = CreateGrid(_agreementHistoryBindingSource);
            AddGridColumn(dgvAgreements, "AgreementNo", "Agreement", 14);
            AddGridColumn(dgvAgreements, "PropertyName", "Property", 18);
            AddGridColumn(dgvAgreements, "HouseName", "House", 14);
            AddGridColumn(dgvAgreements, "RoomNo", "Room", 10);
            AddGridColumn(dgvAgreements, "StartDate", "Start", 12);
            AddGridColumn(dgvAgreements, "EndDate", "End", 12);
            AddGridColumn(dgvAgreements, "MonthlyRent", "Rent", 10);
            AddGridColumn(dgvAgreements, "SecurityDeposit", "Deposit", 10);
            AddGridColumn(dgvAgreements, "Status", "Status", 12);
            surface.Controls.Add(dgvAgreements);
            tabAgreements.Controls.Add(surface);
        }

        private void BuildPaymentsTab()
        {
            Panel surface = CreateSurfacePanel();
            surface.Padding = new Padding(0);
            dgvPayments = CreateGrid(_paymentHistoryBindingSource);
            AddGridColumn(dgvPayments, "ReceiptNo", "Receipt", 14);
            AddGridColumn(dgvPayments, "AgreementNo", "Agreement", 12);
            AddGridColumn(dgvPayments, "PaymentDate", "Date", 11);
            AddGridColumn(dgvPayments, "PaidAmount", "Paid", 12);
            AddGridColumn(dgvPayments, "CurrencyCode", "Currency", 8);
            AddGridColumn(dgvPayments, "PaymentMethod", "Method", 14);
            AddGridColumn(dgvPayments, "ExternalReference", "Reference", 14);
            AddGridColumn(dgvPayments, "CollectedByName", "Collector", 13);
            AddGridColumn(dgvPayments, "Status", "Status", 10);
            surface.Controls.Add(dgvPayments);
            tabPayments.Controls.Add(surface);
        }

        private DataGridView CreateGrid(BindingSource source)
        {
            DataGridView grid = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                DataSource = source,
                Dock = DockStyle.Fill,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(226, 232, 240),
                MultiSelect = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            grid.RowTemplate.Height = 34;
            grid.ColumnHeadersHeight = 38;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(71, 85, 105);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.25F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(241, 245, 249);
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(71, 85, 105);
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(15, 23, 42);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.25F);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(239, 246, 255);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(29, 78, 216);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            return grid;
        }

        private void AddGridColumn(DataGridView grid, string propertyName, string headerText, float fillWeight)
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn
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
            panel.Paint += SurfacePanel_Paint;
            return panel;
        }

        private void SurfacePanel_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, ((Panel)sender).ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
        }

        private void AddHeading(TableLayoutPanel layout, Label label)
        {
            layout.Controls.Add(label, 0, layout.Controls.Count);
        }

        private void AddField(TableLayoutPanel layout, string labelText, Control input)
        {
            layout.Controls.Add(CreateFieldLabel(labelText), 0, layout.Controls.Count);
            layout.Controls.Add(input, 0, layout.Controls.Count);
        }

        private Label CreateEditorHeading(string text)
        {
            return new Label
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Margin = new Padding(0, 0, 0, 14),
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft
            };
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
                Width = EditorWidth
            };
        }

        private TextBox CreateInput()
        {
            TextBox input = new TextBox
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5F),
                Margin = new Padding(0, 0, 0, 12),
                Width = EditorWidth
            };
            input.Enter += Input_Enter;
            input.Leave += Input_Leave;
            return input;
        }

        private TextBox CreateMultilineInput()
        {
            TextBox input = CreateInput();
            input.Height = 72;
            input.Multiline = true;
            input.ScrollBars = ScrollBars.Vertical;
            return input;
        }

        private ComboBox CreateCombo()
        {
            ComboBox combo = new ComboBox
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F),
                Margin = new Padding(0, 0, 0, 12),
                Width = EditorWidth
            };
            combo.Enter += Input_Enter;
            combo.Leave += Input_Leave;
            return combo;
        }

        private TableLayoutPanel CreateActionRow()
        {
            TableLayoutPanel row = new TableLayoutPanel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                ColumnCount = 3,
                Height = 38,
                Margin = new Padding(0, 6, 0, 0),
                RowCount = 1,
                Width = EditorWidth
            };

            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

            btnSave.Margin = new Padding(0, 0, 6, 0);
            btnToggleStatus.Margin = new Padding(6, 0, 6, 0);
            btnBlacklist.Margin = new Padding(6, 0, 0, 0);

            row.Controls.Add(btnSave, 0, 0);
            row.Controls.Add(btnToggleStatus, 1, 0);
            row.Controls.Add(btnBlacklist, 2, 0);
            return row;
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
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                Height = 34,
                Text = text,
                UseVisualStyleBackColor = false,
                Width = 96
            };
            button.FlatAppearance.BorderSize = 1;
            return button;
        }
    }
}
