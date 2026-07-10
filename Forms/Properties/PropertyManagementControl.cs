using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Housing_rental.BLL;
using Housing_rental.Models;

namespace Housing_rental.Forms.Properties
{
    public partial class PropertyManagementControl : UserControl
    {
        private readonly PropertyService _propertyService;
        private readonly BindingSource _propertyBindingSource;
        private readonly BindingSource _houseBindingSource;
        private readonly BindingSource _roomBindingSource;
        private readonly BindingSource _occupancyBindingSource;
        private readonly List<SplitContainer> _splitContainers;

        private bool _loading;
        private int _selectedPropertyId;
        private int _selectedHouseId;
        private int _selectedRoomId;

        public PropertyManagementControl()
        {
            _propertyService = new PropertyService();
            _propertyBindingSource = new BindingSource();
            _houseBindingSource = new BindingSource();
            _roomBindingSource = new BindingSource();
            _occupancyBindingSource = new BindingSource();
            _splitContainers = new List<SplitContainer>();

            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _loading = true;
            LoadPropertyCombos(0, 0, 0);
            LoadFilterHouseCombo(0);
            LoadRoomHouseCombo(0, 0);
            _loading = false;

            RefreshAll();
            StartNewProperty();
            StartNewHouse();
            StartNewRoom();
            AdjustSplitContainers();

            SetPlaceholder(txtSearch, "Search properties, houses or rooms...");
        }

        private void RefreshAll()
        {
            LoadProperties();
            LoadHouses();
            LoadRooms();
            LoadOccupancy();
        }

        private void LoadProperties()
        {
            ServiceResult<List<Property>> result = _propertyService.SearchProperties(txtSearch.Text, chkIncludeInactive.Checked);

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                return;
            }

            _propertyBindingSource.DataSource = result.Data;
            ConfigurePropertyGridColumns();
            SetStatus(result.Message, false);
        }

        private void LoadHouses()
        {
            int? propertyId = GetSelectedId(cmbFilterProperty);
            ServiceResult<List<House>> result = _propertyService.SearchHouses(propertyId, txtSearch.Text, chkIncludeInactive.Checked);

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                return;
            }

            _houseBindingSource.DataSource = result.Data;
            ConfigureHouseGridColumns();
            SetStatus(result.Message, false);
        }

        private void LoadRooms()
        {
            ServiceResult<List<Room>> result = _propertyService.SearchRooms(
                GetSelectedId(cmbFilterProperty),
                GetSelectedId(cmbFilterHouse),
                GetSelectedStatus(),
                txtSearch.Text);

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                return;
            }

            _roomBindingSource.DataSource = result.Data;
            ConfigureRoomGridColumns();
            SetStatus(result.Message, false);
        }

        private void LoadOccupancy()
        {
            ServiceResult<DataTable> result = _propertyService.GetRoomOccupancy(
                GetSelectedId(cmbFilterProperty),
                GetSelectedId(cmbFilterHouse),
                GetSelectedStatus(),
                txtSearch.Text);

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                return;
            }

            _occupancyBindingSource.DataSource = result.Data;
            ConfigureOccupancyGridColumns();
            SetStatus(result.Message, false);
        }

        private void LoadPropertyCombos(int filterPropertyId, int housePropertyId, int roomPropertyId)
        {
            ServiceResult<List<Property>> result = _propertyService.GetActiveProperties();

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                return;
            }

            BindProperties(cmbFilterProperty, result.Data, true, filterPropertyId);
            BindProperties(cmbHouseProperty, result.Data, false, housePropertyId);
            BindProperties(cmbRoomProperty, result.Data, false, roomPropertyId);
        }

        private void LoadFilterHouseCombo(int selectedHouseId)
        {
            int propertyId = GetSelectedId(cmbFilterProperty) ?? 0;
            ServiceResult<List<House>> result = propertyId > 0
                ? _propertyService.GetActiveHousesByPropertyId(propertyId)
                : _propertyService.SearchHouses(null, string.Empty, false);

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                return;
            }

            BindHouses(cmbFilterHouse, result.Data, true, selectedHouseId);
        }

        private void LoadRoomHouseCombo(int propertyId, int selectedHouseId)
        {
            ServiceResult<List<House>> result = propertyId > 0
                ? _propertyService.GetActiveHousesByPropertyId(propertyId)
                : ServiceResult<List<House>>.Success(new List<House>(), "Select a property to load houses.");

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                return;
            }

            BindHouses(cmbRoomHouse, result.Data, false, selectedHouseId);
        }

        private void BindProperties(ComboBox combo, List<Property> properties, bool includeAll, int selectedId)
        {
            List<Property> items = new List<Property>();

            if (includeAll)
            {
                items.Add(new Property { PropertyId = 0, PropertyName = "All properties" });
            }

            items.AddRange(properties);
            combo.DataSource = null;
            combo.DisplayMember = "PropertyName";
            combo.ValueMember = "PropertyId";
            combo.DataSource = items;

            if (selectedId > 0)
            {
                combo.SelectedValue = selectedId;
            }
            else if (combo.Items.Count > 0)
            {
                combo.SelectedIndex = 0;
            }
        }

        private void BindHouses(ComboBox combo, List<House> houses, bool includeAll, int selectedId)
        {
            List<House> items = new List<House>();

            if (includeAll)
            {
                items.Add(new House { HouseId = 0, HouseName = "All houses" });
            }

            items.AddRange(houses);
            combo.DataSource = null;
            combo.DisplayMember = "HouseName";
            combo.ValueMember = "HouseId";
            combo.DataSource = items;

            if (selectedId > 0)
            {
                combo.SelectedValue = selectedId;
            }
            else if (combo.Items.Count > 0)
            {
                combo.SelectedIndex = 0;
            }
        }

        private void StartNewProperty()
        {
            _selectedPropertyId = 0;
            lblPropertyMode.Text = "Create Property";
            btnSaveProperty.Text = "Create Property";
            txtPropertyName.Clear();
            txtPropertyAddress.Clear();
            txtPropertyCity.Clear();
            txtPropertyDescription.Clear();
            chkPropertyActive.Checked = true;
            btnTogglePropertyActive.Enabled = false;
            ResetButtonToSecondary(btnTogglePropertyActive);
            txtPropertyName.Focus();
        }

        private void StartNewHouse()
        {
            _selectedHouseId = 0;
            lblHouseMode.Text = "Create House / Unit";
            btnSaveHouse.Text = "Create House";
            txtHouseName.Clear();
            txtHouseFloor.Clear();
            txtHouseDescription.Clear();
            chkHouseActive.Checked = true;
            btnToggleHouseActive.Enabled = false;
            ResetButtonToSecondary(btnToggleHouseActive);

            if (cmbHouseProperty.Items.Count > 0)
            {
                cmbHouseProperty.SelectedIndex = 0;
            }
        }

        private void StartNewRoom()
        {
            _selectedRoomId = 0;
            lblRoomMode.Text = "Create Room";
            btnSaveRoom.Text = "Create Room";
            txtRoomNo.Clear();
            cmbRoomType.SelectedIndex = cmbRoomType.Items.Count > 0 ? 0 : -1;
            nudMonthlyRent.Value = 1;
            cmbRoomStatus.SelectedItem = "Available";
            txtRoomDescription.Clear();
            btnSetAvailable.Enabled = false;
            btnSetMaintenance.Enabled = false;
            btnSetInactive.Enabled = false;
            HighlightRoomStatusButtons(null);

            if (cmbRoomProperty.Items.Count > 0)
            {
                cmbRoomProperty.SelectedIndex = 0;
                LoadRoomHouseCombo(GetSelectedId(cmbRoomProperty) ?? 0, 0);
            }
        }

        private void LoadSelectedProperty(Property property)
        {
            if (property == null)
            {
                return;
            }

            _selectedPropertyId = property.PropertyId;
            lblPropertyMode.Text = "Edit Property";
            btnSaveProperty.Text = "Save Property";
            txtPropertyName.Text = property.PropertyName;
            txtPropertyAddress.Text = property.Address;
            txtPropertyCity.Text = property.City;
            txtPropertyDescription.Text = property.Description;
            chkPropertyActive.Checked = property.IsActive;
            btnTogglePropertyActive.Enabled = true;
            btnTogglePropertyActive.Text = property.IsActive ? "Deactivate" : "Activate";
            UpdateButtonActiveStateColor(btnTogglePropertyActive, property.IsActive);
        }

        private void LoadSelectedHouse(House house)
        {
            if (house == null)
            {
                return;
            }

            _selectedHouseId = house.HouseId;
            lblHouseMode.Text = "Edit House / Unit";
            btnSaveHouse.Text = "Save House";
            cmbHouseProperty.SelectedValue = house.PropertyId;
            txtHouseName.Text = house.HouseName;
            txtHouseFloor.Text = house.FloorNo;
            txtHouseDescription.Text = house.Description;
            chkHouseActive.Checked = house.IsActive;
            btnToggleHouseActive.Enabled = true;
            btnToggleHouseActive.Text = house.IsActive ? "Deactivate" : "Activate";
            UpdateButtonActiveStateColor(btnToggleHouseActive, house.IsActive);
        }

        private void LoadSelectedRoom(Room room)
        {
            if (room == null)
            {
                return;
            }

            _selectedRoomId = room.RoomId;
            lblRoomMode.Text = "Edit Room";
            btnSaveRoom.Text = "Save Room";

            House parentHouse = FindHouse(room.HouseId);
            if (parentHouse != null)
            {
                cmbRoomProperty.SelectedValue = parentHouse.PropertyId;
                LoadRoomHouseCombo(parentHouse.PropertyId, room.HouseId);
            }

            txtRoomNo.Text = room.RoomNo;
            cmbRoomType.SelectedItem = string.IsNullOrWhiteSpace(room.RoomType) ? "Single" : room.RoomType;
            nudMonthlyRent.Value = NormalizeRent(room.MonthlyRent);
            cmbRoomStatus.SelectedItem = room.Status;
            txtRoomDescription.Text = room.Description;
            btnSetAvailable.Enabled = true;
            btnSetMaintenance.Enabled = true;
            btnSetInactive.Enabled = true;
            HighlightRoomStatusButtons(room.Status);
        }

        private Property ReadPropertyFromForm()
        {
            return new Property
            {
                PropertyId = _selectedPropertyId,
                PropertyName = txtPropertyName.Text,
                Address = txtPropertyAddress.Text,
                City = txtPropertyCity.Text,
                Description = txtPropertyDescription.Text,
                IsActive = chkPropertyActive.Checked
            };
        }

        private House ReadHouseFromForm()
        {
            return new House
            {
                HouseId = _selectedHouseId,
                PropertyId = GetSelectedId(cmbHouseProperty) ?? 0,
                HouseName = txtHouseName.Text,
                FloorNo = txtHouseFloor.Text,
                Description = txtHouseDescription.Text,
                IsActive = chkHouseActive.Checked
            };
        }

        private Room ReadRoomFromForm()
        {
            return new Room
            {
                RoomId = _selectedRoomId,
                HouseId = GetSelectedId(cmbRoomHouse) ?? 0,
                RoomNo = txtRoomNo.Text,
                RoomType = cmbRoomType.Text,
                MonthlyRent = nudMonthlyRent.Value,
                Status = Convert.ToString(cmbRoomStatus.SelectedItem),
                Description = txtRoomDescription.Text
            };
        }

        private void SaveProperty()
        {
            Property property = ReadPropertyFromForm();
            ServiceResult result = _selectedPropertyId == 0
                ? _propertyService.CreateProperty(property)
                : _propertyService.UpdateProperty(property);

            HandleSaveResult(result);

            if (result.IsSuccess)
            {
                int selectedId = _selectedPropertyId;
                RefreshPropertyRelatedData();
                if (selectedId > 0)
                {
                    SelectGridRowByPropertyId(selectedId);
                }
            }
        }

        private void SaveHouse()
        {
            House house = ReadHouseFromForm();
            ServiceResult result = _selectedHouseId == 0
                ? _propertyService.CreateHouse(house)
                : _propertyService.UpdateHouse(house);

            HandleSaveResult(result);

            if (result.IsSuccess)
            {
                int selectedId = _selectedHouseId;
                RefreshHouseRelatedData();
                if (selectedId > 0)
                {
                    SelectGridRowByHouseId(selectedId);
                }
            }
        }

        private void SaveRoom()
        {
            Room room = ReadRoomFromForm();
            ServiceResult result = _selectedRoomId == 0
                ? _propertyService.CreateRoom(room)
                : _propertyService.UpdateRoom(room);

            HandleSaveResult(result);

            if (result.IsSuccess)
            {
                int selectedId = _selectedRoomId;
                LoadRooms();
                LoadOccupancy();
                if (selectedId > 0)
                {
                    SelectGridRowByRoomId(selectedId);
                }
            }
        }

        private void RefreshPropertyRelatedData()
        {
            int filterPropertyId = GetSelectedId(cmbFilterProperty) ?? 0;
            int housePropertyId = GetSelectedId(cmbHouseProperty) ?? 0;
            int roomPropertyId = GetSelectedId(cmbRoomProperty) ?? 0;

            _loading = true;
            LoadPropertyCombos(filterPropertyId, housePropertyId, roomPropertyId);
            LoadFilterHouseCombo(GetSelectedId(cmbFilterHouse) ?? 0);
            LoadRoomHouseCombo(GetSelectedId(cmbRoomProperty) ?? 0, GetSelectedId(cmbRoomHouse) ?? 0);
            _loading = false;

            LoadProperties();
            LoadHouses();
            LoadRooms();
            LoadOccupancy();
        }

        private void RefreshHouseRelatedData()
        {
            _loading = true;
            LoadFilterHouseCombo(GetSelectedId(cmbFilterHouse) ?? 0);
            LoadRoomHouseCombo(GetSelectedId(cmbRoomProperty) ?? 0, GetSelectedId(cmbRoomHouse) ?? 0);
            _loading = false;

            LoadHouses();
            LoadRooms();
            LoadOccupancy();
        }

        private void HandleSaveResult(ServiceResult result)
        {
            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                MessageBox.Show(result.Message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetStatus(result.Message, false);
        }

        private void ChangePropertyStatus()
        {
            Property property = _propertyBindingSource.Current as Property;

            if (property == null)
            {
                SetStatus("Please select a property first.", true);
                return;
            }

            bool newStatus = !property.IsActive;
            if (!ConfirmStatusChange(newStatus ? "activate this property" : "deactivate this property"))
            {
                return;
            }

            ServiceResult result = _propertyService.SetPropertyActiveStatus(property.PropertyId, newStatus);
            HandleSaveResult(result);

            if (result.IsSuccess)
            {
                RefreshPropertyRelatedData();
            }
        }

        private void ChangeHouseStatus()
        {
            House house = _houseBindingSource.Current as House;

            if (house == null)
            {
                SetStatus("Please select a house first.", true);
                return;
            }

            bool newStatus = !house.IsActive;
            if (!ConfirmStatusChange(newStatus ? "activate this house" : "deactivate this house"))
            {
                return;
            }

            ServiceResult result = _propertyService.SetHouseActiveStatus(house.HouseId, newStatus);
            HandleSaveResult(result);

            if (result.IsSuccess)
            {
                RefreshHouseRelatedData();
            }
        }

        private void ChangeRoomStatus(string status)
        {
            Room room = _roomBindingSource.Current as Room;

            if (room == null)
            {
                SetStatus("Please select a room first.", true);
                return;
            }

            if (!ConfirmStatusChange("set this room to " + status))
            {
                return;
            }

            ServiceResult result = _propertyService.SetRoomStatus(room.RoomId, status);
            HandleSaveResult(result);

            if (result.IsSuccess)
            {
                LoadRooms();
                LoadOccupancy();
            }
        }

        private bool ConfirmStatusChange(string action)
        {
            return MessageBox.Show(
                "Are you sure you want to " + action + "?",
                "Confirm Status Change",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private void RefreshActiveTab()
        {
            if (_loading)
            {
                return;
            }

            if (tabMain.SelectedTab == tabProperties)
            {
                LoadProperties();
            }
            else if (tabMain.SelectedTab == tabHouses)
            {
                LoadHouses();
            }
            else if (tabMain.SelectedTab == tabRooms)
            {
                LoadRooms();
            }
            else
            {
                LoadOccupancy();
            }
        }

        private void ConfigurePropertyGridColumns()
        {
            if (dgvProperties.Columns.Count > 0)
            {
                return;
            }

            AddGridColumn(dgvProperties, "PropertyName", "Property", 25);
            AddGridColumn(dgvProperties, "City", "City", 16);
            AddGridColumn(dgvProperties, "Address", "Address", 30);
            AddGridColumn(dgvProperties, "IsActive", "Active", 10);
            AddGridColumn(dgvProperties, "CreatedAt", "Created", 16);
        }

        private void ConfigureHouseGridColumns()
        {
            if (dgvHouses.Columns.Count > 0)
            {
                return;
            }

            AddGridColumn(dgvHouses, "HouseName", "House / Unit", 30);
            AddGridColumn(dgvHouses, "PropertyId", "Property ID", 14);
            AddGridColumn(dgvHouses, "FloorNo", "Floor", 14);
            AddGridColumn(dgvHouses, "IsActive", "Active", 10);
            AddGridColumn(dgvHouses, "CreatedAt", "Created", 16);
        }

        private void ConfigureRoomGridColumns()
        {
            if (dgvRooms.Columns.Count > 0)
            {
                return;
            }

            AddGridColumn(dgvRooms, "RoomNo", "Room", 18);
            AddGridColumn(dgvRooms, "HouseId", "House ID", 12);
            AddGridColumn(dgvRooms, "RoomType", "Type", 16);
            AddGridColumn(dgvRooms, "MonthlyRent", "Rent", 14);
            AddGridColumn(dgvRooms, "Status", "Status", 16);
            AddGridColumn(dgvRooms, "CreatedAt", "Created", 16);
        }

        private void ConfigureOccupancyGridColumns()
        {
            if (dgvOccupancy.Columns.Count > 0)
            {
                return;
            }

            AddGridColumn(dgvOccupancy, "PropertyName", "Property", 18);
            AddGridColumn(dgvOccupancy, "HouseName", "House", 14);
            AddGridColumn(dgvOccupancy, "RoomNo", "Room", 10);
            AddGridColumn(dgvOccupancy, "RoomType", "Type", 12);
            AddGridColumn(dgvOccupancy, "MonthlyRent", "Rent", 10);
            AddGridColumn(dgvOccupancy, "RoomStatus", "Room Status", 14);
            AddGridColumn(dgvOccupancy, "TenantName", "Tenant", 18);
            AddGridColumn(dgvOccupancy, "AgreementNo", "Agreement", 14);
            AddGridColumn(dgvOccupancy, "EndDate", "End Date", 12);
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

        private int? GetSelectedId(ComboBox combo)
        {
            if (combo.SelectedValue == null)
            {
                return null;
            }

            int value;
            if (!int.TryParse(combo.SelectedValue.ToString(), out value) || value <= 0)
            {
                return null;
            }

            return value;
        }

        private string GetSelectedStatus()
        {
            string value = Convert.ToString(cmbFilterStatus.SelectedItem);
            return string.IsNullOrWhiteSpace(value) || value == "All statuses" ? string.Empty : value;
        }

        private House FindHouse(int houseId)
        {
            ServiceResult<List<House>> result = _propertyService.SearchHouses(null, string.Empty, true);

            if (!result.IsSuccess)
            {
                return null;
            }

            foreach (House house in result.Data)
            {
                if (house.HouseId == houseId)
                {
                    return house;
                }
            }

            return null;
        }

        private decimal NormalizeRent(decimal value)
        {
            if (value < nudMonthlyRent.Minimum)
            {
                return nudMonthlyRent.Minimum;
            }

            if (value > nudMonthlyRent.Maximum)
            {
                return nudMonthlyRent.Maximum;
            }

            return value;
        }

        private void SelectGridRowByPropertyId(int propertyId)
        {
            foreach (DataGridViewRow row in dgvProperties.Rows)
            {
                Property property = row.DataBoundItem as Property;
                if (property != null && property.PropertyId == propertyId)
                {
                    row.Selected = true;
                    dgvProperties.CurrentCell = row.Cells[0];
                    return;
                }
            }
        }

        private void SelectGridRowByHouseId(int houseId)
        {
            foreach (DataGridViewRow row in dgvHouses.Rows)
            {
                House house = row.DataBoundItem as House;
                if (house != null && house.HouseId == houseId)
                {
                    row.Selected = true;
                    dgvHouses.CurrentCell = row.Cells[0];
                    return;
                }
            }
        }

        private void SelectGridRowByRoomId(int roomId)
        {
            foreach (DataGridViewRow row in dgvRooms.Rows)
            {
                Room room = row.DataBoundItem as Room;
                if (room != null && room.RoomId == roomId)
                {
                    row.Selected = true;
                    dgvRooms.CurrentCell = row.Cells[0];
                    return;
                }
            }
        }

        private void SetStatus(string message, bool isError)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = isError ? Color.FromArgb(220, 38, 38) : Color.FromArgb(22, 163, 74);
        }

        private void RegisterSplitContainer(SplitContainer splitContainer)
        {
            _splitContainers.Add(splitContainer);
            splitContainer.Resize += SplitContainer_Resize;
        }

        private void SplitContainer_Resize(object sender, EventArgs e)
        {
            AdjustSplitContainer(sender as SplitContainer);
        }

        private void AdjustSplitContainers()
        {
            foreach (SplitContainer splitContainer in _splitContainers)
            {
                AdjustSplitContainer(splitContainer);
            }
        }

        private void AdjustSplitContainer(SplitContainer splitContainer)
        {
            if (splitContainer == null || splitContainer.Width <= 0)
            {
                return;
            }

            int minimumListWidth = 220;
            int desiredEditorWidth = 380;
            int availableWidth = splitContainer.Width - splitContainer.SplitterWidth;

            if (availableWidth <= minimumListWidth + 120)
            {
                return;
            }

            int editorWidth = Math.Min(desiredEditorWidth, Math.Max(260, availableWidth / 3));
            int splitterDistance = availableWidth - editorWidth;
            int minimumDistance = Math.Max(splitContainer.Panel1MinSize, minimumListWidth);
            int maximumDistance = splitContainer.Width - splitContainer.Panel2MinSize - splitContainer.SplitterWidth;

            if (maximumDistance < minimumDistance)
            {
                return;
            }

            splitContainer.SplitterDistance = Math.Max(minimumDistance, Math.Min(splitterDistance, maximumDistance));
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            RefreshActiveTab();
        }

        private void ChkIncludeInactive_CheckedChanged(object sender, EventArgs e)
        {
            RefreshAll();
        }

        private void CmbFilterProperty_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }

            _loading = true;
            LoadFilterHouseCombo(0);
            _loading = false;
            RefreshActiveTab();
        }

        private void CmbFilterHouse_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshActiveTab();
        }

        private void CmbFilterStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshActiveTab();
        }

        private void CmbRoomProperty_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }

            LoadRoomHouseCombo(GetSelectedId(cmbRoomProperty) ?? 0, 0);
        }

        private void TabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshActiveTab();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            RefreshAll();
        }

        private void BtnNewProperty_Click(object sender, EventArgs e)
        {
            StartNewProperty();
        }

        private void BtnSaveProperty_Click(object sender, EventArgs e)
        {
            SaveProperty();
        }

        private void BtnTogglePropertyActive_Click(object sender, EventArgs e)
        {
            ChangePropertyStatus();
        }

        private void BtnNewHouse_Click(object sender, EventArgs e)
        {
            StartNewHouse();
        }

        private void BtnSaveHouse_Click(object sender, EventArgs e)
        {
            SaveHouse();
        }

        private void BtnToggleHouseActive_Click(object sender, EventArgs e)
        {
            ChangeHouseStatus();
        }

        private void BtnNewRoom_Click(object sender, EventArgs e)
        {
            StartNewRoom();
        }

        private void BtnSaveRoom_Click(object sender, EventArgs e)
        {
            SaveRoom();
        }

        private void BtnSetAvailable_Click(object sender, EventArgs e)
        {
            ChangeRoomStatus("Available");
        }

        private void BtnSetMaintenance_Click(object sender, EventArgs e)
        {
            ChangeRoomStatus("Maintenance");
        }

        private void BtnSetInactive_Click(object sender, EventArgs e)
        {
            ChangeRoomStatus("Inactive");
        }

        private void DgvProperties_SelectionChanged(object sender, EventArgs e)
        {
            LoadSelectedProperty(_propertyBindingSource.Current as Property);
        }

        private void DgvHouses_SelectionChanged(object sender, EventArgs e)
        {
            LoadSelectedHouse(_houseBindingSource.Current as House);
        }

        private void DgvRooms_SelectionChanged(object sender, EventArgs e)
        {
            LoadSelectedRoom(_roomBindingSource.Current as Room);
        }

        // Cue banner placeholder support
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string lParam);

        private const int EM_SETCUEBANNER = 0x1501;

        private void SetPlaceholder(TextBox textBox, string placeholder)
        {
            if (textBox != null)
            {
                IntPtr handle = textBox.Handle;
                SendMessage(handle, EM_SETCUEBANNER, 0, placeholder);
            }
        }

        // Focus Highlight handlers
        private void Input_Enter(object sender, EventArgs e)
        {
            Control ctrl = sender as Control;
            if (ctrl != null)
            {
                ctrl.BackColor = Color.FromArgb(240, 249, 255); // Soft blue tint on focus
            }
        }

        private void Input_Leave(object sender, EventArgs e)
        {
            Control ctrl = sender as Control;
            if (ctrl != null)
            {
                ctrl.BackColor = Color.White;
            }
        }

        // Owner-draw TabControl custom rendering
        private void TabMain_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tc = (TabControl)sender;
            if (tc.TabPages.Count == 0 || e.Index < 0 || e.Index >= tc.TabPages.Count) return;

            TabPage page = tc.TabPages[e.Index];
            bool selected = tc.SelectedIndex == e.Index;

            // Colors
            Color backColor = selected ? Color.White : Color.FromArgb(241, 245, 249);
            Color textColor = selected ? Color.FromArgb(37, 99, 235) : Color.FromArgb(100, 116, 139);
            Font textFont = new Font("Segoe UI", 9.5F, selected ? FontStyle.Bold : FontStyle.Regular);

            // Fill tab button background
            using (SolidBrush brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            // Draw Centered text
            TextRenderer.DrawText(e.Graphics, page.Text, textFont, e.Bounds, textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            // Draw accent active indicator line at the bottom
            if (selected)
            {
                using (Pen pen = new Pen(Color.FromArgb(37, 99, 235), 3))
                {
                    e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 2, e.Bounds.Right, e.Bounds.Bottom - 2);
                }
            }
            else
            {
                // Draw a subtle border bottom line
                using (Pen pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                {
                    e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
                }
            }
        }

        // Room Status button highlighting
        private void HighlightRoomStatusButtons(string currentStatus)
        {
            ResetButtonToSecondary(btnSetAvailable);
            ResetButtonToSecondary(btnSetMaintenance);
            ResetButtonToSecondary(btnSetInactive);

            if (string.Equals(currentStatus, "Available", StringComparison.OrdinalIgnoreCase))
            {
                HighlightButton(btnSetAvailable, Color.FromArgb(22, 163, 74)); // Success green
            }
            else if (string.Equals(currentStatus, "Maintenance", StringComparison.OrdinalIgnoreCase))
            {
                HighlightButton(btnSetMaintenance, Color.FromArgb(217, 119, 6)); // Amber orange
            }
            else if (string.Equals(currentStatus, "Inactive", StringComparison.OrdinalIgnoreCase))
            {
                HighlightButton(btnSetInactive, Color.FromArgb(220, 38, 38)); // Red
            }
        }

        private void ResetButtonToSecondary(Button btn)
        {
            if (btn == null) return;
            btn.BackColor = Color.FromArgb(241, 245, 249);
            btn.ForeColor = Color.FromArgb(51, 65, 85);
            btn.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(226, 232, 240);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(203, 213, 225);
        }

        private void HighlightButton(Button btn, Color color)
        {
            if (btn == null) return;
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderColor = color;
            btn.FlatAppearance.MouseOverBackColor = color;
            btn.FlatAppearance.MouseDownBackColor = color;
        }

        private void UpdateButtonActiveStateColor(Button btn, bool isActive)
        {
            if (btn == null) return;
            if (isActive)
            {
                btn.ForeColor = Color.FromArgb(220, 38, 38); // Warning red for deactivation
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(254, 242, 242);
                btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(254, 226, 226);
            }
            else
            {
                btn.ForeColor = Color.FromArgb(22, 163, 74); // Success green for activation
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 253, 250);
                btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(204, 251, 241);
            }
        }
    }
}
