using System.Drawing;
using System.Windows.Forms;

namespace Housing_rental.Forms.Agreements
{
    public partial class AgreementManagementControl
    {
        private TableLayoutPanel pageLayout;
        private Panel toolbarPanel;
        private TextBox txtSearch;
        private ComboBox cmbFilterStatus;
        private ComboBox cmbFilterProperty;
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private Button btnRefresh;
        private Button btnExpireDue;
        private TabControl tabMain;
        private TabPage tabAgreements;
        private TabPage tabDetails;
        private TabPage tabPayments;
        private TabPage tabExpiring;
        private SplitContainer splitContainer;
        private DataGridView dgvAgreements;
        private DataGridView dgvDetails;
        private DataGridView dgvPayments;
        private DataGridView dgvExpiring;
        private Label lblStatus;
        private Label lblMode;
        private TextBox txtAgreementNo;
        private ComboBox cmbTenant;
        private ComboBox cmbRoom;
        private DateTimePicker dtpStart;
        private DateTimePicker dtpEnd;
        private NumericUpDown nudMonthlyRent;
        private NumericUpDown nudSecurityDeposit;
        private ComboBox cmbAgreementStatus;
        private TextBox txtNotes;
        private TextBox txtLifecycleReason;
        private Button btnNew;
        private Button btnSaveDraft;
        private Button btnSaveActive;
        private Button btnUpdateNotes;
        private Button btnActivate;
        private Button btnRenew;
        private Button btnTerminate;
        private Button btnCancel;
        private const int EditorWidth = 340;

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
                ColumnCount = 7,
                RowCount = 1,
                BackColor = Color.White
            };
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145F));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 165F));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 122F));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 122F));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86F));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118F));

            txtSearch = CreateInput();
            txtSearch.Dock = DockStyle.Fill;
            txtSearch.Margin = new Padding(0, 8, 10, 8);
            txtSearch.TextChanged += TxtSearch_TextChanged;

            cmbFilterStatus = CreateCombo();
            cmbFilterStatus.Dock = DockStyle.Fill;
            cmbFilterStatus.Margin = new Padding(0, 8, 10, 8);
            cmbFilterStatus.Items.AddRange(new object[] { "All statuses", "Draft", "Active", "Expired", "Terminated", "Cancelled" });
            cmbFilterStatus.SelectedIndexChanged += CmbFilterStatus_SelectedIndexChanged;

            cmbFilterProperty = CreateCombo();
            cmbFilterProperty.Dock = DockStyle.Fill;
            cmbFilterProperty.Margin = new Padding(0, 8, 10, 8);
            cmbFilterProperty.SelectedIndexChanged += CmbFilterProperty_SelectedIndexChanged;

            dtpFrom = CreateFilterDatePicker();
            dtpFrom.Margin = new Padding(0, 8, 10, 8);
            dtpFrom.ValueChanged += DtpFilter_ValueChanged;

            dtpTo = CreateFilterDatePicker();
            dtpTo.Margin = new Padding(0, 8, 10, 8);
            dtpTo.ValueChanged += DtpFilter_ValueChanged;

            btnRefresh = CreateSecondaryButton("Refresh");
            btnRefresh.Dock = DockStyle.Fill;
            btnRefresh.Margin = new Padding(0, 8, 10, 8);
            btnRefresh.Click += BtnRefresh_Click;

            btnExpireDue = CreateSecondaryButton("Expire Due");
            btnExpireDue.Dock = DockStyle.Fill;
            btnExpireDue.Margin = new Padding(0, 8, 0, 8);
            btnExpireDue.Click += BtnExpireDue_Click;

            toolbar.Controls.Add(txtSearch, 0, 0);
            toolbar.Controls.Add(cmbFilterStatus, 1, 0);
            toolbar.Controls.Add(cmbFilterProperty, 2, 0);
            toolbar.Controls.Add(dtpFrom, 3, 0);
            toolbar.Controls.Add(dtpTo, 4, 0);
            toolbar.Controls.Add(btnRefresh, 5, 0);
            toolbar.Controls.Add(btnExpireDue, 6, 0);
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

            tabAgreements = new TabPage("Agreements") { BackColor = Color.FromArgb(248, 250, 252) };
            tabDetails = new TabPage("Details") { BackColor = Color.FromArgb(248, 250, 252) };
            tabPayments = new TabPage("Payments") { BackColor = Color.FromArgb(248, 250, 252) };
            tabExpiring = new TabPage("Expiring") { BackColor = Color.FromArgb(248, 250, 252) };

            BuildAgreementsTab();
            BuildDetailsTab();
            BuildPaymentsTab();
            BuildExpiringTab();

            tabMain.TabPages.Add(tabAgreements);
            tabMain.TabPages.Add(tabDetails);
            tabMain.TabPages.Add(tabPayments);
            tabMain.TabPages.Add(tabExpiring);
        }

        private void BuildAgreementsTab()
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
            dgvAgreements = CreateGrid(_agreementBindingSource);
            dgvAgreements.SelectionChanged += DgvAgreements_SelectionChanged;
            AddGridColumn(dgvAgreements, "AgreementNo", "Agreement", 14);
            AddGridColumn(dgvAgreements, "TenantName", "Tenant", 18);
            AddGridColumn(dgvAgreements, "PropertyName", "Property", 16);
            AddGridColumn(dgvAgreements, "RoomNo", "Room", 9);
            AddGridColumn(dgvAgreements, "StartDate", "Start", 10);
            AddGridColumn(dgvAgreements, "EndDate", "End", 10);
            AddGridColumn(dgvAgreements, "MonthlyRent", "Rent", 10);
            AddGridColumn(dgvAgreements, "AgreementStatus", "Status", 11);
            AddGridColumn(dgvAgreements, "TotalBalance", "Balance", 10);
            listPanel.Controls.Add(dgvAgreements);

            Panel editorPanel = CreateSurfacePanel();
            editorPanel.AutoScroll = true;
            BuildEditorPanel(editorPanel);

            splitContainer.Panel1.Controls.Add(listPanel);
            splitContainer.Panel2.Controls.Add(editorPanel);
            tabAgreements.Controls.Add(splitContainer);
        }

        private void BuildEditorPanel(Panel parent)
        {
            TableLayoutPanel editor = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 25,
                Padding = new Padding(22),
                BackColor = Color.White
            };

            for (int i = 0; i < 25; i++)
            {
                editor.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            lblMode = CreateEditorHeading("Create Agreement");
            txtAgreementNo = CreateInput();
            cmbTenant = CreateCombo();
            cmbRoom = CreateCombo();
            dtpStart = CreateDatePicker();
            dtpEnd = CreateDatePicker();
            nudMonthlyRent = CreateMoneyInput();
            nudSecurityDeposit = CreateMoneyInput();
            nudSecurityDeposit.Minimum = 0;
            cmbAgreementStatus = CreateCombo();
            cmbAgreementStatus.Items.AddRange(new object[] { "Draft", "Active", "Expired", "Terminated", "Cancelled" });
            cmbAgreementStatus.Enabled = false;
            txtNotes = CreateMultilineInput();
            txtLifecycleReason = CreateInput();

            btnNew = CreateSecondaryButton("New");
            btnSaveDraft = CreatePrimaryButton("Save Draft");
            btnSaveActive = CreatePrimaryButton("Save Active");
            btnUpdateNotes = CreateSecondaryButton("Save Notes");
            btnActivate = CreateSecondaryButton("Activate");
            btnRenew = CreateSecondaryButton("Renew");
            btnTerminate = CreateSecondaryButton("Terminate");
            btnCancel = CreateSecondaryButton("Cancel");

            btnNew.Click += BtnNew_Click;
            btnSaveDraft.Click += BtnSaveDraft_Click;
            btnSaveActive.Click += BtnSaveActive_Click;
            btnUpdateNotes.Click += BtnUpdateNotes_Click;
            btnActivate.Click += BtnActivate_Click;
            btnRenew.Click += BtnRenew_Click;
            btnTerminate.Click += BtnTerminate_Click;
            btnCancel.Click += BtnCancel_Click;

            AddHeading(editor, lblMode);
            AddField(editor, "Agreement Number", txtAgreementNo);
            AddField(editor, "Tenant", cmbTenant);
            AddField(editor, "Available Room", cmbRoom);
            AddField(editor, "Start Date", dtpStart);
            AddField(editor, "End Date", dtpEnd);
            AddField(editor, "Monthly Rent", nudMonthlyRent);
            AddField(editor, "Security Deposit", nudSecurityDeposit);
            AddField(editor, "Status", cmbAgreementStatus);
            AddField(editor, "Notes", txtNotes);
            AddField(editor, "Lifecycle Reason", txtLifecycleReason);
            editor.Controls.Add(CreatePrimaryActionRow(), 0, editor.Controls.Count);
            editor.Controls.Add(CreateLifecycleActionRow(), 0, editor.Controls.Count);

            parent.Controls.Add(editor);
        }

        private void BuildDetailsTab()
        {
            Panel surface = CreateSurfacePanel();
            dgvDetails = CreateGrid(_detailBindingSource);
            AddGridColumn(dgvDetails, "AgreementNo", "Agreement", 12);
            AddGridColumn(dgvDetails, "TenantName", "Tenant", 16);
            AddGridColumn(dgvDetails, "TenantPhone", "Phone", 12);
            AddGridColumn(dgvDetails, "PropertyName", "Property", 14);
            AddGridColumn(dgvDetails, "HouseName", "House", 12);
            AddGridColumn(dgvDetails, "RoomNo", "Room", 8);
            AddGridColumn(dgvDetails, "AgreementStatus", "Status", 10);
            AddGridColumn(dgvDetails, "TotalDue", "Due", 10);
            AddGridColumn(dgvDetails, "TotalPaid", "Paid", 10);
            AddGridColumn(dgvDetails, "TotalBalance", "Balance", 10);
            surface.Controls.Add(dgvDetails);
            tabDetails.Controls.Add(surface);
        }

        private void BuildPaymentsTab()
        {
            Panel surface = CreateSurfacePanel();
            dgvPayments = CreateGrid(_paymentBindingSource);
            AddGridColumn(dgvPayments, "ReceiptNo", "Receipt", 14);
            AddGridColumn(dgvPayments, "PaymentMonth", "Month", 8);
            AddGridColumn(dgvPayments, "PaymentYear", "Year", 8);
            AddGridColumn(dgvPayments, "DueAmount", "Due", 12);
            AddGridColumn(dgvPayments, "PaidAmount", "Paid", 12);
            AddGridColumn(dgvPayments, "BalanceAmount", "Balance", 12);
            AddGridColumn(dgvPayments, "PaymentDate", "Date", 12);
            AddGridColumn(dgvPayments, "PaymentMethod", "Method", 14);
            AddGridColumn(dgvPayments, "Status", "Status", 12);
            surface.Controls.Add(dgvPayments);
            tabPayments.Controls.Add(surface);
        }

        private void BuildExpiringTab()
        {
            Panel surface = CreateSurfacePanel();
            dgvExpiring = CreateGrid(_expiringBindingSource);
            dgvExpiring.SelectionChanged += DgvExpiring_SelectionChanged;
            AddGridColumn(dgvExpiring, "AgreementNo", "Agreement", 14);
            AddGridColumn(dgvExpiring, "TenantName", "Tenant", 18);
            AddGridColumn(dgvExpiring, "TenantPhone", "Phone", 14);
            AddGridColumn(dgvExpiring, "PropertyName", "Property", 18);
            AddGridColumn(dgvExpiring, "RoomNo", "Room", 10);
            AddGridColumn(dgvExpiring, "EndDate", "End Date", 12);
            AddGridColumn(dgvExpiring, "DaysLeft", "Days Left", 10);
            AddGridColumn(dgvExpiring, "MonthlyRent", "Rent", 10);
            surface.Controls.Add(dgvExpiring);
            tabExpiring.Controls.Add(surface);
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

        private DateTimePicker CreateDatePicker()
        {
            DateTimePicker picker = new DateTimePicker
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Font = new Font("Segoe UI", 9.5F),
                Format = DateTimePickerFormat.Short,
                Margin = new Padding(0, 0, 0, 12),
                Width = EditorWidth
            };
            picker.Enter += Input_Enter;
            picker.Leave += Input_Leave;
            return picker;
        }

        private DateTimePicker CreateFilterDatePicker()
        {
            DateTimePicker picker = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Checked = false
            };
            picker.Enter += Input_Enter;
            picker.Leave += Input_Leave;
            return picker;
        }

        private NumericUpDown CreateMoneyInput()
        {
            NumericUpDown input = new NumericUpDown
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                DecimalPlaces = 2,
                Font = new Font("Segoe UI", 9.5F),
                Maximum = 1000000,
                Minimum = 1,
                Margin = new Padding(0, 0, 0, 12),
                ThousandsSeparator = true,
                Width = EditorWidth
            };
            input.Enter += Input_Enter;
            input.Leave += Input_Leave;
            return input;
        }

        private TableLayoutPanel CreatePrimaryActionRow()
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

            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37.5F));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37.5F));
            AddButtonToRow(row, btnNew, 0, new Padding(0, 0, 6, 0));
            AddButtonToRow(row, btnSaveDraft, 1, new Padding(6, 0, 6, 0));
            AddButtonToRow(row, btnSaveActive, 2, new Padding(6, 0, 0, 0));
            return row;
        }

        private TableLayoutPanel CreateLifecycleActionRow()
        {
            TableLayoutPanel row = new TableLayoutPanel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                ColumnCount = 5,
                Height = 38,
                Margin = new Padding(0, 8, 0, 0),
                RowCount = 1,
                Width = EditorWidth
            };

            for (int i = 0; i < 5; i++)
            {
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            }

            AddButtonToRow(row, btnUpdateNotes, 0, new Padding(0, 0, 4, 0));
            AddButtonToRow(row, btnActivate, 1, new Padding(4, 0, 4, 0));
            AddButtonToRow(row, btnRenew, 2, new Padding(4, 0, 4, 0));
            AddButtonToRow(row, btnTerminate, 3, new Padding(4, 0, 4, 0));
            AddButtonToRow(row, btnCancel, 4, new Padding(4, 0, 0, 0));
            return row;
        }

        private void AddButtonToRow(TableLayoutPanel row, Button button, int column, Padding margin)
        {
            button.Dock = DockStyle.Fill;
            button.Margin = margin;
            row.Controls.Add(button, column, 0);
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
                Font = new Font("Segoe UI Semibold", 8.25F, FontStyle.Bold),
                Height = 34,
                Text = text,
                UseVisualStyleBackColor = false,
                Width = 86
            };
            button.FlatAppearance.BorderSize = 1;
            return button;
        }
    }
}
