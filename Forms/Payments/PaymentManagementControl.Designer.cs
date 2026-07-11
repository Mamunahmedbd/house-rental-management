using System.Drawing;
using System.Windows.Forms;

namespace Housing_rental.Forms.Payments
{
    partial class PaymentManagementControl
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox txtAgreementSearch;
        private Button btnSearchAgreements;
        private Button btnRefresh;
        private DateTimePicker dtpBillingPeriod;
        private Button btnGenerateCharges;
        private TabControl tabMain;
        private TabPage tabCollect;
        private TabPage tabDues;
        private TabPage tabHistory;
        private DataGridView dgvAgreements;
        private DataGridView dgvCharges;
        private Label lblAgreementContext;
        private NumericUpDown numPaymentAmount;
        private DateTimePicker dtpPaymentDate;
        private ComboBox cmbPaymentMethod;
        private TextBox txtExternalReference;
        private TextBox txtRemarks;
        private Button btnAllocateOldest;
        private Button btnClearAllocation;
        private Button btnPostPayment;
        private TextBox txtDuesSearch;
        private ComboBox cmbDuesStatus;
        private CheckBox chkIncludePaid;
        private Button btnFilterDues;
        private DataGridView dgvDues;
        private TextBox txtHistorySearch;
        private ComboBox cmbHistoryStatus;
        private CheckBox chkUseHistoryDates;
        private DateTimePicker dtpHistoryFrom;
        private DateTimePicker dtpHistoryTo;
        private Button btnFilterHistory;
        private Button btnViewPayment;
        private Button btnReversePayment;
        private DataGridView dgvPayments;
        private Label lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            SuspendLayout();

            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(248, 250, 252);
            Font = new Font("Segoe UI", 9F);
            Padding = new Padding(18);
            MinimumSize = new Size(920, 620);

            TableLayoutPanel root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = BackColor,
                ColumnCount = 1,
                RowCount = 3
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

            root.Controls.Add(BuildToolbar(), 0, 0);
            BuildTabs();
            root.Controls.Add(tabMain, 0, 1);

            lblStatus = new Label
            {
                AutoEllipsis = true,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(71, 85, 105),
                Padding = new Padding(4, 8, 4, 0),
                Text = "Ready."
            };
            root.Controls.Add(lblStatus, 0, 2);

            Controls.Add(root);
            Load += PaymentManagementControl_Load;
            ResumeLayout(false);
        }

        private Control BuildToolbar()
        {
            TableLayoutPanel toolbar = new TableLayoutPanel
            {
                BackColor = Color.White,
                ColumnCount = 7,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(10, 6, 10, 6),
                RowCount = 1
            };
            toolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 12F));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112F));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 154F));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 94F));

            txtAgreementSearch = CreateInput("Search agreement, tenant, phone, property, house or room");
            txtAgreementSearch.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtAgreementSearch.Margin = new Padding(0, 7, 8, 7);
            txtAgreementSearch.KeyDown += TxtAgreementSearch_KeyDown;

            btnSearchAgreements = CreateSecondaryButton("Search");
            ConfigureToolbarButton(btnSearchAgreements);
            btnSearchAgreements.Click += BtnSearchAgreements_Click;

            Label monthLabel = new Label
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                AutoSize = false,
                ForeColor = Color.FromArgb(71, 85, 105),
                Height = 32,
                Margin = new Padding(0, 4, 8, 4),
                Text = "Billing month",
                TextAlign = ContentAlignment.MiddleRight
            };

            dtpBillingPeriod = new DateTimePicker
            {
                CustomFormat = "MMM yyyy",
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Format = DateTimePickerFormat.Custom,
                Margin = new Padding(0, 7, 8, 7),
                ShowUpDown = true
            };

            btnGenerateCharges = CreatePrimaryButton("Generate Charges");
            ConfigureToolbarButton(btnGenerateCharges);
            btnGenerateCharges.Click += BtnGenerateCharges_Click;

            btnRefresh = CreateSecondaryButton("Refresh");
            ConfigureToolbarButton(btnRefresh);
            btnRefresh.Click += BtnRefresh_Click;

            toolbar.Controls.Add(txtAgreementSearch, 0, 0);
            toolbar.Controls.Add(btnSearchAgreements, 1, 0);
            toolbar.Controls.Add(new Panel(), 2, 0);
            toolbar.Controls.Add(monthLabel, 3, 0);
            toolbar.Controls.Add(dtpBillingPeriod, 4, 0);
            toolbar.Controls.Add(btnGenerateCharges, 5, 0);
            toolbar.Controls.Add(btnRefresh, 6, 0);
            return toolbar;
        }

        private void BuildTabs()
        {
            tabMain = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F),
                ItemSize = new Size(170, 38),
                SizeMode = TabSizeMode.Fixed
            };

            tabCollect = new TabPage("Collect Payment") { BackColor = Color.FromArgb(248, 250, 252), Padding = new Padding(0, 8, 0, 0) };
            tabDues = new TabPage("Dues & Overdue") { BackColor = Color.FromArgb(248, 250, 252), Padding = new Padding(0, 8, 0, 0) };
            tabHistory = new TabPage("Payment History") { BackColor = Color.FromArgb(248, 250, 252), Padding = new Padding(0, 8, 0, 0) };

            BuildCollectTab();
            BuildDuesTab();
            BuildHistoryTab();
            tabMain.TabPages.Add(tabCollect);
            tabMain.TabPages.Add(tabDues);
            tabMain.TabPages.Add(tabHistory);
        }

        private void BuildCollectTab()
        {
            SplitContainer split = new SplitContainer
            {
                BackColor = Color.FromArgb(226, 232, 240),
                Dock = DockStyle.Fill,
                FixedPanel = FixedPanel.Panel1,
                SplitterWidth = 10
            };
            split.Resize += delegate
            {
                if (split.Width < 760)
                {
                    return;
                }

                int target = System.Math.Min(350, split.Width - 420 - split.SplitterWidth);
                if (target >= 260 && target != split.SplitterDistance)
                {
                    split.SplitterDistance = target;
                }
                split.Panel1MinSize = 260;
                split.Panel2MinSize = 420;
            };

            dgvAgreements = CreateGrid(true);
            AddTextColumn(dgvAgreements, "AgreementNo", "Agreement", 16);
            AddTextColumn(dgvAgreements, "TenantName", "Tenant", 24);
            AddTextColumn(dgvAgreements, "RoomNo", "Room", 10);
            AddTextColumn(dgvAgreements, "TotalBalance", "Balance", 14, "N2");
            AddTextColumn(dgvAgreements, "AgreementStatus", "Status", 12);
            dgvAgreements.SelectionChanged += DgvAgreements_SelectionChanged;
            split.Panel1.Controls.Add(WrapSurface(dgvAgreements));

            TableLayoutPanel collection = new TableLayoutPanel
            {
                BackColor = Color.FromArgb(248, 250, 252),
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                RowCount = 3
            };
            collection.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            collection.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            collection.RowStyles.Add(new RowStyle(SizeType.Absolute, 176F));

            lblAgreementContext = new Label
            {
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(14, 12, 14, 8),
                Text = "Select an active agreement or an agreement with an outstanding balance."
            };
            collection.Controls.Add(lblAgreementContext, 0, 0);

            dgvCharges = CreateGrid(false);
            AddCheckColumn(dgvCharges, "Selected", "Pay", 7);
            AddTextColumn(dgvCharges, "BillingPeriod", "Period", 12, "MMM yyyy", true);
            AddTextColumn(dgvCharges, "DueDate", "Due Date", 12, "dd MMM yyyy", true);
            AddTextColumn(dgvCharges, "Amount", "Charge", 11, "N2", true);
            AddTextColumn(dgvCharges, "PaidAmount", "Paid", 11, "N2", true);
            AddTextColumn(dgvCharges, "BalanceAmount", "Balance", 11, "N2", true);
            AddTextColumn(dgvCharges, "AllocationAmount", "Allocate", 12, "N2", false);
            AddTextColumn(dgvCharges, "CurrencyCode", "Currency", 8, null, true);
            AddTextColumn(dgvCharges, "ChargeStatus", "Status", 10, null, true);
            dgvCharges.CurrentCellDirtyStateChanged += DgvCharges_CurrentCellDirtyStateChanged;
            dgvCharges.CellValueChanged += DgvCharges_CellValueChanged;
            dgvCharges.CellEndEdit += DgvCharges_CellEndEdit;
            collection.Controls.Add(WrapSurface(dgvCharges), 0, 1);
            collection.Controls.Add(BuildPaymentEntry(), 0, 2);

            split.Panel2.Controls.Add(collection);
            tabCollect.Controls.Add(split);
        }

        private Control BuildPaymentEntry()
        {
            FlowLayoutPanel entry = new FlowLayoutPanel
            {
                AutoScroll = true,
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(12, 10, 12, 8),
                WrapContents = true
            };

            numPaymentAmount = new NumericUpDown
            {
                DecimalPlaces = 2,
                Maximum = 999999999999m,
                Minimum = 0,
                ThousandsSeparator = true,
                Width = 140
            };
            dtpPaymentDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 130 };
            cmbPaymentMethod = CreateCombo();
            cmbPaymentMethod.Width = 145;
            cmbPaymentMethod.SelectedIndexChanged += CmbPaymentMethod_SelectedIndexChanged;
            txtExternalReference = CreateInput();
            txtExternalReference.Enabled = false;
            txtExternalReference.Width = 180;
            txtRemarks = CreateInput();
            txtRemarks.Width = 230;

            entry.Controls.Add(CreateFieldBlock("Amount received", numPaymentAmount, 154));
            entry.Controls.Add(CreateFieldBlock("Payment date", dtpPaymentDate, 144));
            entry.Controls.Add(CreateFieldBlock("Method", cmbPaymentMethod, 159));
            entry.Controls.Add(CreateFieldBlock("External reference", txtExternalReference, 194));
            entry.Controls.Add(CreateFieldBlock("Remarks", txtRemarks, 244));

            btnAllocateOldest = CreateSecondaryButton("Allocate Oldest");
            btnAllocateOldest.Width = 126;
            btnAllocateOldest.Click += BtnAllocateOldest_Click;
            btnClearAllocation = CreateSecondaryButton("Clear");
            btnClearAllocation.Width = 78;
            btnClearAllocation.Click += BtnClearAllocation_Click;
            btnPostPayment = CreatePrimaryButton("Post Payment");
            btnPostPayment.Width = 120;
            btnPostPayment.Click += BtnPostPayment_Click;

            Panel actions = new Panel { Height = 64, Width = 350, Margin = new Padding(8, 18, 0, 0) };
            btnPostPayment.Location = new Point(230, 5);
            btnClearAllocation.Location = new Point(144, 5);
            btnAllocateOldest.Location = new Point(10, 5);
            actions.Controls.Add(btnAllocateOldest);
            actions.Controls.Add(btnClearAllocation);
            actions.Controls.Add(btnPostPayment);
            entry.Controls.Add(actions);
            return entry;
        }

        private void BuildDuesTab()
        {
            TableLayoutPanel root = new TableLayoutPanel { ColumnCount = 1, Dock = DockStyle.Fill, RowCount = 2 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            FlowLayoutPanel filters = new FlowLayoutPanel
            {
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 8, 10, 6),
                WrapContents = false
            };
            txtDuesSearch = CreateInput("Search dues");
            txtDuesSearch.Width = 280;
            cmbDuesStatus = CreateCombo();
            cmbDuesStatus.Width = 130;
            chkIncludePaid = new CheckBox { AutoSize = true, Margin = new Padding(12, 8, 10, 0), Text = "Include paid" };
            btnFilterDues = CreateSecondaryButton("Apply Filter");
            btnFilterDues.Width = 100;
            btnFilterDues.Click += BtnFilterDues_Click;
            filters.Controls.Add(txtDuesSearch);
            filters.Controls.Add(cmbDuesStatus);
            filters.Controls.Add(chkIncludePaid);
            filters.Controls.Add(btnFilterDues);
            root.Controls.Add(filters, 0, 0);

            dgvDues = CreateGrid(true);
            AddTextColumn(dgvDues, "AgreementNo", "Agreement", 12);
            AddTextColumn(dgvDues, "TenantName", "Tenant", 18);
            AddTextColumn(dgvDues, "PropertyName", "Property", 14);
            AddTextColumn(dgvDues, "HouseName", "House", 11);
            AddTextColumn(dgvDues, "RoomNo", "Room", 8);
            AddTextColumn(dgvDues, "BillingPeriod", "Period", 10, "MMM yyyy");
            AddTextColumn(dgvDues, "DueDate", "Due Date", 11, "dd MMM yyyy");
            AddTextColumn(dgvDues, "Amount", "Charge", 10, "N2");
            AddTextColumn(dgvDues, "PaidAmount", "Paid", 10, "N2");
            AddTextColumn(dgvDues, "BalanceAmount", "Balance", 10, "N2");
            AddTextColumn(dgvDues, "ChargeStatus", "Status", 10);
            dgvDues.CellDoubleClick += DgvDues_CellDoubleClick;
            root.Controls.Add(WrapSurface(dgvDues), 0, 1);
            tabDues.Controls.Add(root);
        }

        private void BuildHistoryTab()
        {
            TableLayoutPanel root = new TableLayoutPanel { ColumnCount = 1, Dock = DockStyle.Fill, RowCount = 2 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            FlowLayoutPanel filters = new FlowLayoutPanel
            {
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 8, 10, 6),
                WrapContents = false
            };
            txtHistorySearch = CreateInput("Search receipts");
            txtHistorySearch.Width = 220;
            cmbHistoryStatus = CreateCombo();
            cmbHistoryStatus.Width = 105;
            chkUseHistoryDates = new CheckBox { AutoSize = true, Margin = new Padding(10, 8, 4, 0), Text = "Date range" };
            dtpHistoryFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 112 };
            dtpHistoryTo = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 112 };
            btnFilterHistory = CreateSecondaryButton("Apply Filter");
            btnFilterHistory.Width = 96;
            btnFilterHistory.Click += BtnFilterHistory_Click;
            btnViewPayment = CreateSecondaryButton("View Receipt");
            btnViewPayment.Width = 104;
            btnViewPayment.Click += BtnViewPayment_Click;
            btnReversePayment = CreateDangerButton("Reverse");
            btnReversePayment.Width = 86;
            btnReversePayment.Click += BtnReversePayment_Click;

            filters.Controls.Add(txtHistorySearch);
            filters.Controls.Add(cmbHistoryStatus);
            filters.Controls.Add(chkUseHistoryDates);
            filters.Controls.Add(dtpHistoryFrom);
            filters.Controls.Add(dtpHistoryTo);
            filters.Controls.Add(btnFilterHistory);
            filters.Controls.Add(btnViewPayment);
            filters.Controls.Add(btnReversePayment);
            root.Controls.Add(filters, 0, 0);

            dgvPayments = CreateGrid(true);
            AddTextColumn(dgvPayments, "ReceiptNo", "Receipt", 14);
            AddTextColumn(dgvPayments, "PaymentDate", "Date", 10, "dd MMM yyyy");
            AddTextColumn(dgvPayments, "TenantName", "Tenant", 17);
            AddTextColumn(dgvPayments, "AgreementNo", "Agreement", 12);
            AddTextColumn(dgvPayments, "RoomNo", "Room", 7);
            AddTextColumn(dgvPayments, "Amount", "Amount", 10, "N2");
            AddTextColumn(dgvPayments, "CurrencyCode", "Currency", 7);
            AddTextColumn(dgvPayments, "PaymentMethod", "Method", 11);
            AddTextColumn(dgvPayments, "ExternalReference", "Reference", 13);
            AddTextColumn(dgvPayments, "CollectedByName", "Collector", 13);
            AddTextColumn(dgvPayments, "Status", "Status", 9);
            dgvPayments.SelectionChanged += DgvPayments_SelectionChanged;
            root.Controls.Add(WrapSurface(dgvPayments), 0, 1);
            tabHistory.Controls.Add(root);
        }

        private DataGridView CreateGrid(bool readOnly)
        {
            DataGridView grid = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                Dock = DockStyle.Fill,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(226, 232, 240),
                MultiSelect = false,
                ReadOnly = readOnly,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            grid.RowTemplate.Height = 34;
            grid.ColumnHeadersHeight = 38;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(71, 85, 105);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(15, 23, 42);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 64, 175);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            grid.DataError += delegate(object sender, DataGridViewDataErrorEventArgs args)
            {
                args.ThrowException = false;
                args.Cancel = true;
            };
            return grid;
        }

        private static void AddTextColumn(DataGridView grid, string propertyName, string header, float weight, string format = null, bool readOnly = true)
        {
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = propertyName,
                FillWeight = weight,
                HeaderText = header,
                Name = propertyName,
                ReadOnly = readOnly
            };
            if (!string.IsNullOrWhiteSpace(format))
            {
                column.DefaultCellStyle.Format = format;
            }
            grid.Columns.Add(column);
        }

        private static void AddCheckColumn(DataGridView grid, string propertyName, string header, float weight)
        {
            grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = propertyName,
                FillWeight = weight,
                HeaderText = header,
                Name = propertyName,
                ReadOnly = false
            });
        }

        private static Panel WrapSurface(Control content)
        {
            Panel panel = new Panel
            {
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Padding = new Padding(1),
                Margin = new Padding(0, 4, 0, 0)
            };
            panel.Controls.Add(content);
            return panel;
        }

        private static Panel CreateFieldBlock(string labelText, Control input, int width)
        {
            Panel panel = new Panel { Height = 62, Width = width, Margin = new Padding(0, 0, 8, 0) };
            Label label = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(0, 0),
                Text = labelText
            };
            input.Location = new Point(0, 22);
            input.Height = 28;
            panel.Controls.Add(label);
            panel.Controls.Add(input);
            return panel;
        }

        private static TextBox CreateInput(string placeholder = null)
        {
            TextBox input = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.25F),
                Margin = new Padding(0, 0, 8, 0),
                Width = 180
            };
            if (!string.IsNullOrWhiteSpace(placeholder))
            {
                input.AccessibleDescription = placeholder;
            }
            return input;
        }

        private static ComboBox CreateCombo()
        {
            return new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.25F),
                Margin = new Padding(0, 0, 8, 0),
                Width = 140
            };
        }

        private static Button CreatePrimaryButton(string text)
        {
            Button button = CreateBaseButton(text);
            button.BackColor = Color.FromArgb(37, 99, 235);
            button.ForeColor = Color.White;
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private static Button CreateSecondaryButton(string text)
        {
            Button button = CreateBaseButton(text);
            button.BackColor = Color.FromArgb(241, 245, 249);
            button.ForeColor = Color.FromArgb(51, 65, 85);
            button.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            return button;
        }

        private static Button CreateDangerButton(string text)
        {
            Button button = CreateBaseButton(text);
            button.BackColor = Color.FromArgb(254, 242, 242);
            button.ForeColor = Color.FromArgb(185, 28, 28);
            button.FlatAppearance.BorderColor = Color.FromArgb(254, 202, 202);
            return button;
        }

        private static Button CreateBaseButton(string text)
        {
            Button button = new Button
            {
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                Height = 32,
                Margin = new Padding(4, 0, 4, 0),
                Text = text,
                UseVisualStyleBackColor = false,
                Width = 92
            };
            button.FlatAppearance.BorderSize = 1;
            return button;
        }

        private static void ConfigureToolbarButton(Button button)
        {
            button.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            button.AutoEllipsis = true;
            button.Height = 32;
            button.Margin = new Padding(4, 4, 4, 4);
            button.TextAlign = ContentAlignment.MiddleCenter;
        }
    }
}
