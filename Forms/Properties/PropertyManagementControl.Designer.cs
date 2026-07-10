using System;
using System.Drawing;
using System.Windows.Forms;

namespace Housing_rental.Forms.Properties
{
    public partial class PropertyManagementControl
    {
        private TableLayoutPanel pageLayout;
        private TableLayoutPanel filterLayout;
        private TextBox txtSearch;
        private ComboBox cmbFilterProperty;
        private ComboBox cmbFilterHouse;
        private ComboBox cmbFilterStatus;
        private CheckBox chkIncludeInactive;
        private Button btnRefresh;
        private TabControl tabMain;
        private TabPage tabProperties;
        private TabPage tabHouses;
        private TabPage tabRooms;
        private TabPage tabOccupancy;
        private DataGridView dgvProperties;
        private DataGridView dgvHouses;
        private DataGridView dgvRooms;
        private DataGridView dgvOccupancy;
        private Label lblStatus;
        private Label lblPropertyMode;
        private Label lblHouseMode;
        private Label lblRoomMode;
        private TextBox txtPropertyName;
        private TextBox txtPropertyAddress;
        private TextBox txtPropertyCity;
        private TextBox txtPropertyDescription;
        private CheckBox chkPropertyActive;
        private Button btnNewProperty;
        private Button btnSaveProperty;
        private Button btnTogglePropertyActive;
        private ComboBox cmbHouseProperty;
        private TextBox txtHouseName;
        private TextBox txtHouseFloor;
        private TextBox txtHouseDescription;
        private CheckBox chkHouseActive;
        private Button btnNewHouse;
        private Button btnSaveHouse;
        private Button btnToggleHouseActive;
        private ComboBox cmbRoomProperty;
        private ComboBox cmbRoomHouse;
        private TextBox txtRoomNo;
        private ComboBox cmbRoomType;
        private NumericUpDown nudMonthlyRent;
        private ComboBox cmbRoomStatus;
        private TextBox txtRoomDescription;
        private Button btnNewRoom;
        private Button btnSaveRoom;
        private Button btnSetAvailable;
        private Button btnSetMaintenance;
        private Button btnSetInactive;
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
            pageLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            pageLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            pageLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));

            BuildFilters();
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

            pageLayout.Controls.Add(filterLayout, 0, 0);
            pageLayout.Controls.Add(tabMain, 0, 1);
            pageLayout.Controls.Add(lblStatus, 0, 2);
            Controls.Add(pageLayout);
        }

        private void BuildFilters()
        {
            filterLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 1,
                BackColor = Color.FromArgb(248, 250, 252)
            };
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));

            txtSearch = CreateInput();
            txtSearch.Margin = new Padding(0, 8, 10, 8);
            txtSearch.TextChanged += TxtSearch_TextChanged;

            cmbFilterProperty = CreateCombo();
            cmbFilterProperty.Margin = new Padding(0, 8, 10, 8);
            cmbFilterProperty.SelectedIndexChanged += CmbFilterProperty_SelectedIndexChanged;

            cmbFilterHouse = CreateCombo();
            cmbFilterHouse.Margin = new Padding(0, 8, 10, 8);
            cmbFilterHouse.SelectedIndexChanged += CmbFilterHouse_SelectedIndexChanged;

            cmbFilterStatus = CreateCombo();
            cmbFilterStatus.Margin = new Padding(0, 8, 10, 8);
            cmbFilterStatus.Items.AddRange(new object[] { "All statuses", "Available", "Occupied", "Maintenance", "Inactive" });
            cmbFilterStatus.SelectedIndex = 0;
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
            btnRefresh.Margin = new Padding(0, 8, 0, 8);
            btnRefresh.Click += BtnRefresh_Click;

            filterLayout.Controls.Add(txtSearch, 0, 0);
            filterLayout.Controls.Add(cmbFilterProperty, 1, 0);
            filterLayout.Controls.Add(cmbFilterHouse, 2, 0);
            filterLayout.Controls.Add(cmbFilterStatus, 3, 0);
            filterLayout.Controls.Add(chkIncludeInactive, 4, 0);
            filterLayout.Controls.Add(btnRefresh, 5, 0);
        }

        private void BuildTabs()
        {
            tabMain = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F)
            };
            tabMain.SelectedIndexChanged += TabMain_SelectedIndexChanged;

            tabProperties = new TabPage("Properties") { BackColor = Color.FromArgb(248, 250, 252) };
            tabHouses = new TabPage("Houses / Units") { BackColor = Color.FromArgb(248, 250, 252) };
            tabRooms = new TabPage("Rooms") { BackColor = Color.FromArgb(248, 250, 252) };
            tabOccupancy = new TabPage("Occupancy") { BackColor = Color.FromArgb(248, 250, 252) };

            BuildPropertiesTab();
            BuildHousesTab();
            BuildRoomsTab();
            BuildOccupancyTab();

            tabMain.TabPages.Add(tabProperties);
            tabMain.TabPages.Add(tabHouses);
            tabMain.TabPages.Add(tabRooms);
            tabMain.TabPages.Add(tabOccupancy);
        }

        private void BuildPropertiesTab()
        {
            SplitContainer split = CreateSplitContainer();
            dgvProperties = CreateGrid(_propertyBindingSource);
            dgvProperties.SelectionChanged += DgvProperties_SelectionChanged;

            Panel editorPanel = CreateSurfacePanel();
            editorPanel.AutoScroll = true;
            TableLayoutPanel editor = CreateEditorLayout(15);
            lblPropertyMode = CreateEditorHeading("Create Property");
            txtPropertyName = CreateInput();
            txtPropertyAddress = CreateInput();
            txtPropertyCity = CreateInput();
            txtPropertyDescription = CreateMultilineInput();
            chkPropertyActive = CreateCheckBox("Active property");
            btnNewProperty = CreateSecondaryButton("New");
            btnSaveProperty = CreatePrimaryButton("Create Property");
            btnTogglePropertyActive = CreateSecondaryButton("Deactivate");

            btnNewProperty.Click += BtnNewProperty_Click;
            btnSaveProperty.Click += BtnSaveProperty_Click;
            btnTogglePropertyActive.Click += BtnTogglePropertyActive_Click;

            AddHeading(editor, lblPropertyMode);
            AddField(editor, "Property Name", txtPropertyName);
            AddField(editor, "Address", txtPropertyAddress);
            AddField(editor, "City", txtPropertyCity);
            AddField(editor, "Description", txtPropertyDescription);
            editor.Controls.Add(chkPropertyActive, 0, editor.Controls.Count);
            editor.Controls.Add(CreateActionRow(btnNewProperty, btnSaveProperty, btnTogglePropertyActive), 0, editor.Controls.Count);

            editorPanel.Controls.Add(editor);
            split.Panel1.Controls.Add(dgvProperties);
            split.Panel2.Controls.Add(editorPanel);
            tabProperties.Controls.Add(split);
        }

        private void BuildHousesTab()
        {
            SplitContainer split = CreateSplitContainer();
            dgvHouses = CreateGrid(_houseBindingSource);
            dgvHouses.SelectionChanged += DgvHouses_SelectionChanged;

            Panel editorPanel = CreateSurfacePanel();
            editorPanel.AutoScroll = true;
            TableLayoutPanel editor = CreateEditorLayout(15);
            lblHouseMode = CreateEditorHeading("Create House / Unit");
            cmbHouseProperty = CreateCombo();
            txtHouseName = CreateInput();
            txtHouseFloor = CreateInput();
            txtHouseDescription = CreateMultilineInput();
            chkHouseActive = CreateCheckBox("Active house / unit");
            btnNewHouse = CreateSecondaryButton("New");
            btnSaveHouse = CreatePrimaryButton("Create House");
            btnToggleHouseActive = CreateSecondaryButton("Deactivate");

            btnNewHouse.Click += BtnNewHouse_Click;
            btnSaveHouse.Click += BtnSaveHouse_Click;
            btnToggleHouseActive.Click += BtnToggleHouseActive_Click;

            AddHeading(editor, lblHouseMode);
            AddField(editor, "Property", cmbHouseProperty);
            AddField(editor, "House / Unit Name", txtHouseName);
            AddField(editor, "Floor Number", txtHouseFloor);
            AddField(editor, "Description", txtHouseDescription);
            editor.Controls.Add(chkHouseActive, 0, editor.Controls.Count);
            editor.Controls.Add(CreateActionRow(btnNewHouse, btnSaveHouse, btnToggleHouseActive), 0, editor.Controls.Count);

            editorPanel.Controls.Add(editor);
            split.Panel1.Controls.Add(dgvHouses);
            split.Panel2.Controls.Add(editorPanel);
            tabHouses.Controls.Add(split);
        }

        private void BuildRoomsTab()
        {
            SplitContainer split = CreateSplitContainer();
            dgvRooms = CreateGrid(_roomBindingSource);
            dgvRooms.SelectionChanged += DgvRooms_SelectionChanged;

            Panel editorPanel = CreateSurfacePanel();
            editorPanel.AutoScroll = true;
            TableLayoutPanel editor = CreateEditorLayout(19);
            lblRoomMode = CreateEditorHeading("Create Room");
            cmbRoomProperty = CreateCombo();
            cmbRoomProperty.SelectedIndexChanged += CmbRoomProperty_SelectedIndexChanged;
            cmbRoomHouse = CreateCombo();
            txtRoomNo = CreateInput();
            cmbRoomType = CreateCombo();
            cmbRoomType.Items.AddRange(new object[] { "Single", "Double", "Master", "Shared", "Studio", "Other" });
            nudMonthlyRent = new NumericUpDown
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
            cmbRoomStatus = CreateCombo();
            cmbRoomStatus.Items.AddRange(new object[] { "Available", "Occupied", "Maintenance", "Inactive" });
            txtRoomDescription = CreateMultilineInput();
            btnNewRoom = CreateSecondaryButton("New");
            btnSaveRoom = CreatePrimaryButton("Create Room");
            btnSetAvailable = CreateSecondaryButton("Available");
            btnSetMaintenance = CreateSecondaryButton("Maintenance");
            btnSetInactive = CreateSecondaryButton("Inactive");

            btnNewRoom.Click += BtnNewRoom_Click;
            btnSaveRoom.Click += BtnSaveRoom_Click;
            btnSetAvailable.Click += BtnSetAvailable_Click;
            btnSetMaintenance.Click += BtnSetMaintenance_Click;
            btnSetInactive.Click += BtnSetInactive_Click;

            AddHeading(editor, lblRoomMode);
            AddField(editor, "Property", cmbRoomProperty);
            AddField(editor, "House / Unit", cmbRoomHouse);
            AddField(editor, "Room Number", txtRoomNo);
            AddField(editor, "Room Type", cmbRoomType);
            AddField(editor, "Monthly Rent", nudMonthlyRent);
            AddField(editor, "Status", cmbRoomStatus);
            AddField(editor, "Description", txtRoomDescription);
            editor.Controls.Add(CreateActionRow(btnNewRoom, btnSaveRoom, null), 0, editor.Controls.Count);
            editor.Controls.Add(CreateStatusActionRow(), 0, editor.Controls.Count);

            editorPanel.Controls.Add(editor);
            split.Panel1.Controls.Add(dgvRooms);
            split.Panel2.Controls.Add(editorPanel);
            tabRooms.Controls.Add(split);
        }

        private void BuildOccupancyTab()
        {
            Panel surface = CreateSurfacePanel();
            surface.Padding = new Padding(18);
            dgvOccupancy = CreateGrid(_occupancyBindingSource);
            surface.Controls.Add(dgvOccupancy);
            tabOccupancy.Controls.Add(surface);
        }

        private SplitContainer CreateSplitContainer()
        {
            SplitContainer split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 250, 252),
                FixedPanel = FixedPanel.Panel2,
                Panel1MinSize = 80,
                Panel2MinSize = 80,
                SplitterWidth = 14
            };

            RegisterSplitContainer(split);
            return split;
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

        private TableLayoutPanel CreateEditorLayout(int rows)
        {
            TableLayoutPanel layout = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = rows,
                Padding = new Padding(22),
                BackColor = Color.White
            };

            for (int i = 0; i < rows; i++)
            {
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            return layout;
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
            ControlPaint.DrawBorder(e.Graphics, ((Panel)sender).ClientRectangle,
                Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
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
            return new TextBox
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5F),
                Margin = new Padding(0, 0, 0, 12),
                Width = EditorWidth
            };
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
            return new ComboBox
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F),
                Margin = new Padding(0, 0, 0, 12),
                Width = EditorWidth
            };
        }

        private CheckBox CreateCheckBox(string text)
        {
            return new CheckBox
            {
                AutoSize = true,
                Checked = true,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(51, 65, 85),
                Margin = new Padding(0, 4, 0, 16),
                Text = text
            };
        }

        private TableLayoutPanel CreateActionRow(Button left, Button middle, Button right)
        {
            TableLayoutPanel row = new TableLayoutPanel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                ColumnCount = right == null ? 2 : 3,
                Height = 38,
                Margin = new Padding(0, 6, 0, 0),
                RowCount = 1,
                Width = EditorWidth
            };

            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, right == null ? 38F : 30F));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, right == null ? 62F : 40F));
            if (right != null)
            {
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            }

            left.Dock = DockStyle.Fill;
            left.Margin = new Padding(0, 0, 6, 0);
            middle.Dock = DockStyle.Fill;
            middle.Margin = new Padding(6, 0, right == null ? 0 : 6, 0);

            row.Controls.Add(left, 0, 0);
            row.Controls.Add(middle, 1, 0);

            if (right != null)
            {
                right.Dock = DockStyle.Fill;
                right.Margin = new Padding(6, 0, 0, 0);
                row.Controls.Add(right, 2, 0);
            }

            return row;
        }

        private TableLayoutPanel CreateStatusActionRow()
        {
            TableLayoutPanel row = new TableLayoutPanel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                ColumnCount = 3,
                Height = 38,
                Margin = new Padding(0, 8, 0, 0),
                RowCount = 1,
                Width = EditorWidth
            };

            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));

            btnSetAvailable.Dock = DockStyle.Fill;
            btnSetAvailable.Margin = new Padding(0, 0, 5, 0);
            btnSetMaintenance.Dock = DockStyle.Fill;
            btnSetMaintenance.Margin = new Padding(5, 0, 5, 0);
            btnSetInactive.Dock = DockStyle.Fill;
            btnSetInactive.Margin = new Padding(5, 0, 0, 0);

            row.Controls.Add(btnSetAvailable, 0, 0);
            row.Controls.Add(btnSetMaintenance, 1, 0);
            row.Controls.Add(btnSetInactive, 2, 0);

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
