using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Housing_rental.BLL;
using Housing_rental.Models;

namespace Housing_rental.Forms.Agreements
{
    public partial class AgreementManagementControl : UserControl
    {
        private readonly AgreementService _agreementService;
        private readonly PropertyService _propertyService;
        private readonly BindingSource _agreementBindingSource;
        private readonly BindingSource _detailBindingSource;
        private readonly BindingSource _paymentBindingSource;
        private readonly BindingSource _expiringBindingSource;

        private bool _loading;
        private int _selectedAgreementId;

        public AgreementManagementControl()
        {
            _agreementService = new AgreementService();
            _propertyService = new PropertyService();
            _agreementBindingSource = new BindingSource();
            _detailBindingSource = new BindingSource();
            _paymentBindingSource = new BindingSource();
            _expiringBindingSource = new BindingSource();

            InitializeComponent();
            cmbRoom.SelectedIndexChanged += CmbRoom_SelectedIndexChanged;
            dtpStart.ValueChanged += DtpStart_ValueChanged;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _loading = true;
            cmbFilterStatus.SelectedIndex = 0;
            dtpStart.Value = DateTime.Today;
            dtpEnd.Value = DateTime.Today.AddMonths(12);
            cmbAgreementStatus.SelectedItem = "Draft";
            _loading = false;

            SetPlaceholder(txtSearch, "Search agreements by number, tenant, phone, property, house or room...");
            LoadLookups(0, null, 0, null);
            LoadFilterProperties();
            LoadAgreements();
            LoadExpiringAgreements();
            StartNewAgreement();
            AdjustSplitter();
        }

        private void LoadAgreements()
        {
            ServiceResult<DataTable> result = _agreementService.GetAgreementDirectory(
                txtSearch == null ? string.Empty : txtSearch.Text,
                GetSelectedStatusFilter(),
                GetSelectedId(cmbFilterProperty),
                null,
                dtpFrom.Checked ? (DateTime?)dtpFrom.Value.Date : null,
                dtpTo.Checked ? (DateTime?)dtpTo.Value.Date : null);

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                return;
            }

            _agreementBindingSource.DataSource = result.Data;
            SetStatus(result.Data.Rows.Count == 0 ? "No agreements found for the selected filters." : result.Message, false);
        }

        private void LoadSelectedAgreement(int agreementId)
        {
            if (agreementId <= 0)
            {
                return;
            }

            ServiceResult<RentalAgreement> agreementResult = _agreementService.GetAgreementById(agreementId);
            if (!agreementResult.IsSuccess)
            {
                SetStatus(agreementResult.Message, true);
                return;
            }

            ServiceResult<DataTable> detailResult = _agreementService.GetAgreementDetails(agreementId);
            string tenantName = null;
            string roomText = null;

            if (detailResult.IsSuccess && detailResult.Data.Rows.Count > 0)
            {
                DataRow row = detailResult.Data.Rows[0];
                tenantName = Convert.ToString(row["TenantName"]);
                roomText = Convert.ToString(row["RoomNo"]);
            }

            RentalAgreement agreement = agreementResult.Data;
            _selectedAgreementId = agreement.AgreementId;
            LoadLookups(agreement.TenantId, tenantName, agreement.RoomId, roomText);

            lblMode.Text = agreement.Status == "Draft" ? "Edit Draft Agreement" : "View Agreement";
            txtAgreementNo.Text = agreement.AgreementNo;
            SetComboValue(cmbTenant, agreement.TenantId);
            SetComboValue(cmbRoom, agreement.RoomId);
            dtpStart.Value = agreement.StartDate;
            dtpEnd.Value = agreement.EndDate;
            nudMonthlyRent.Value = ClampMoney(agreement.MonthlyRent, nudMonthlyRent);
            nudSecurityDeposit.Value = ClampMoney(agreement.SecurityDeposit, nudSecurityDeposit);
            cmbAgreementStatus.SelectedItem = agreement.Status;
            txtNotes.Text = agreement.Notes;
            txtLifecycleReason.Clear();

            LoadAgreementRelatedData(agreementId);
            UpdateEditorState(agreement.Status);
        }

        private void LoadAgreementRelatedData(int agreementId)
        {
            if (agreementId <= 0)
            {
                _detailBindingSource.DataSource = CreateEmptyTable();
                _paymentBindingSource.DataSource = CreateEmptyTable();
                return;
            }

            ServiceResult<DataTable> detailResult = _agreementService.GetAgreementDetails(agreementId);
            if (detailResult.IsSuccess)
            {
                _detailBindingSource.DataSource = detailResult.Data;
            }
            else
            {
                SetStatus(detailResult.Message, true);
            }

            ServiceResult<DataTable> paymentResult = _agreementService.GetAgreementPaymentHistory(agreementId);
            if (paymentResult.IsSuccess)
            {
                _paymentBindingSource.DataSource = paymentResult.Data;
            }
            else
            {
                SetStatus(paymentResult.Message, true);
            }
        }

        private void LoadExpiringAgreements()
        {
            ServiceResult<DataTable> result = _agreementService.GetExpiringAgreements(30);

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                return;
            }

            _expiringBindingSource.DataSource = result.Data;
        }

        private void LoadFilterProperties()
        {
            ServiceResult<List<Property>> result = _propertyService.GetActiveProperties();
            List<LookupItem> items = new List<LookupItem>();
            items.Add(new LookupItem(0, "All properties"));

            if (result.IsSuccess)
            {
                foreach (Property property in result.Data)
                {
                    items.Add(new LookupItem(property.PropertyId, property.PropertyName));
                }
            }

            cmbFilterProperty.DataSource = items;
            cmbFilterProperty.DisplayMember = "Text";
            cmbFilterProperty.ValueMember = "Id";
        }

        private void LoadLookups(int selectedTenantId, string selectedTenantName, int selectedRoomId, string selectedRoomText)
        {
            bool previousLoading = _loading;
            _loading = true;

            List<LookupItem> tenantItems = new List<LookupItem>();
            tenantItems.Add(new LookupItem(0, "Select tenant"));

            ServiceResult<List<Tenant>> tenantResult = _agreementService.GetEligibleTenants();
            if (tenantResult.IsSuccess)
            {
                foreach (Tenant tenant in tenantResult.Data)
                {
                    tenantItems.Add(new LookupItem(tenant.TenantId, tenant.FullName));
                }
            }

            AddMissingLookup(tenantItems, selectedTenantId, selectedTenantName);
            cmbTenant.DataSource = tenantItems;
            cmbTenant.DisplayMember = "Text";
            cmbTenant.ValueMember = "Id";

            List<LookupItem> roomItems = new List<LookupItem>();
            roomItems.Add(new LookupItem(0, "Select room"));

            ServiceResult<List<Room>> roomResult = _agreementService.GetEligibleRooms();
            if (roomResult.IsSuccess)
            {
                foreach (Room room in roomResult.Data)
                {
                    roomItems.Add(new LookupItem(room.RoomId, room.RoomNo + " - " + room.MonthlyRent.ToString("C"), room.MonthlyRent));
                }
            }

            AddMissingLookup(roomItems, selectedRoomId, selectedRoomText);
            cmbRoom.DataSource = roomItems;
            cmbRoom.DisplayMember = "Text";
            cmbRoom.ValueMember = "Id";

            SetComboValue(cmbTenant, selectedTenantId);
            SetComboValue(cmbRoom, selectedRoomId);
            _loading = previousLoading;
        }

        private void StartNewAgreement()
        {
            _selectedAgreementId = 0;
            lblMode.Text = "Create Agreement";
            txtAgreementNo.Text = GenerateAgreementNo(dtpStart.Value);
            LoadLookups(0, null, 0, null);
            SetComboValue(cmbTenant, 0);
            SetComboValue(cmbRoom, 0);
            dtpStart.Value = DateTime.Today;
            dtpEnd.Value = DateTime.Today.AddMonths(12);
            nudMonthlyRent.Value = 1;
            nudSecurityDeposit.Value = 0;
            cmbAgreementStatus.SelectedItem = "Draft";
            txtNotes.Clear();
            txtLifecycleReason.Clear();
            _detailBindingSource.DataSource = CreateEmptyTable();
            _paymentBindingSource.DataSource = CreateEmptyTable();
            UpdateEditorState("Draft");
            txtAgreementNo.Focus();
        }

        private RentalAgreement ReadAgreementFromForm()
        {
            return new RentalAgreement
            {
                AgreementId = _selectedAgreementId,
                AgreementNo = txtAgreementNo.Text,
                TenantId = GetSelectedId(cmbTenant) ?? 0,
                RoomId = GetSelectedId(cmbRoom) ?? 0,
                StartDate = dtpStart.Value.Date,
                EndDate = dtpEnd.Value.Date,
                MonthlyRent = nudMonthlyRent.Value,
                SecurityDeposit = nudSecurityDeposit.Value,
                Status = Convert.ToString(cmbAgreementStatus.SelectedItem),
                Notes = txtNotes.Text
            };
        }

        private void SaveDraft()
        {
            RentalAgreement agreement = ReadAgreementFromForm();
            ServiceResult result = _selectedAgreementId == 0
                ? _agreementService.CreateDraftAgreement(agreement)
                : _agreementService.UpdateDraftAgreement(agreement);

            HandleSaveResult(result, agreement.AgreementNo);
        }

        private void SaveActive()
        {
            RentalAgreement agreement = ReadAgreementFromForm();
            ServiceResult result;

            if (_selectedAgreementId == 0)
            {
                result = _agreementService.CreateAndActivateAgreement(agreement);
            }
            else
            {
                result = _agreementService.UpdateDraftAgreement(agreement);
                if (result.IsSuccess)
                {
                    result = _agreementService.ActivateAgreement(_selectedAgreementId);
                }
            }

            HandleSaveResult(result, agreement.AgreementNo);
        }

        private void HandleSaveResult(ServiceResult result, string agreementNo)
        {
            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                MessageBox.Show(result.Message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetStatus(result.Message, false);
            RefreshAllData();
            SelectAgreementByNumber(agreementNo);
        }

        private void RefreshAllData()
        {
            LoadFilterProperties();
            LoadAgreements();
            LoadExpiringAgreements();
        }

        private void SelectAgreementByNumber(string agreementNo)
        {
            if (string.IsNullOrWhiteSpace(agreementNo))
            {
                return;
            }

            foreach (DataGridViewRow row in dgvAgreements.Rows)
            {
                DataRowView view = row.DataBoundItem as DataRowView;
                if (view == null)
                {
                    continue;
                }

                if (string.Equals(Convert.ToString(view["AgreementNo"]), agreementNo, StringComparison.OrdinalIgnoreCase))
                {
                    row.Selected = true;
                    dgvAgreements.CurrentCell = row.Cells[0];
                    LoadSelectedAgreement(Convert.ToInt32(view["AgreementId"]));
                    return;
                }
            }
        }

        private void ChangeAgreementLifecycle(string action)
        {
            if (_selectedAgreementId <= 0)
            {
                SetStatus("Please select an agreement first.", true);
                return;
            }

            if (!ConfirmLifecycleAction(action))
            {
                return;
            }

            string reason = txtLifecycleReason.Text;
            ServiceResult result;

            if (action == "Activate")
            {
                result = _agreementService.ActivateAgreement(_selectedAgreementId);
            }
            else if (action == "Terminate")
            {
                result = _agreementService.TerminateAgreement(_selectedAgreementId, reason);
            }
            else if (action == "Cancel")
            {
                result = _agreementService.CancelAgreement(_selectedAgreementId, reason);
            }
            else
            {
                result = _agreementService.ExpireAgreement(_selectedAgreementId);
            }

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                MessageBox.Show(result.Message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetStatus(result.Message, false);
            int agreementId = _selectedAgreementId;
            RefreshAllData();
            SelectAgreementById(agreementId);
        }

        private void RenewSelectedAgreement()
        {
            if (_selectedAgreementId <= 0)
            {
                SetStatus("Please select an active agreement first.", true);
                return;
            }

            if (!ConfirmLifecycleAction("Renew"))
            {
                return;
            }

            ServiceResult result = _agreementService.RenewAgreement(
                _selectedAgreementId,
                dtpEnd.Value.Date,
                nudMonthlyRent.Value,
                nudSecurityDeposit.Value,
                txtNotes.Text);

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                MessageBox.Show(result.Message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetStatus(result.Message, false);
            RefreshAllData();
        }

        private void SelectAgreementById(int agreementId)
        {
            foreach (DataGridViewRow row in dgvAgreements.Rows)
            {
                DataRowView view = row.DataBoundItem as DataRowView;
                if (view != null && Convert.ToInt32(view["AgreementId"]) == agreementId)
                {
                    row.Selected = true;
                    dgvAgreements.CurrentCell = row.Cells[0];
                    LoadSelectedAgreement(agreementId);
                    return;
                }
            }
        }

        private void UpdateEditorState(string status)
        {
            bool isNew = _selectedAgreementId == 0;
            bool isDraft = status == "Draft";
            bool isActive = status == "Active";
            bool canEditContract = isNew || isDraft;

            txtAgreementNo.ReadOnly = !canEditContract;
            cmbTenant.Enabled = canEditContract;
            cmbRoom.Enabled = canEditContract;
            dtpStart.Enabled = canEditContract;
            dtpEnd.Enabled = canEditContract || isActive;
            nudMonthlyRent.Enabled = canEditContract || isActive;
            nudSecurityDeposit.Enabled = canEditContract || isActive;
            btnSaveDraft.Enabled = canEditContract;
            btnSaveActive.Enabled = canEditContract;
            btnUpdateNotes.Enabled = !isNew;
            btnActivate.Enabled = !isNew && isDraft;
            btnRenew.Enabled = !isNew && isActive;
            btnTerminate.Enabled = !isNew && isActive;
            btnCancel.Enabled = !isNew && (isDraft || isActive);

            ResetButtonToSecondary(btnActivate);
            ResetButtonToSecondary(btnRenew);
            ResetButtonToSecondary(btnTerminate);
            ResetButtonToSecondary(btnCancel);

            if (btnActivate.Enabled)
            {
                HighlightButton(btnActivate, Color.FromArgb(22, 163, 74));
            }

            if (btnRenew.Enabled)
            {
                HighlightButton(btnRenew, Color.FromArgb(37, 99, 235));
            }

            if (btnTerminate.Enabled)
            {
                btnTerminate.ForeColor = Color.FromArgb(220, 38, 38);
                btnTerminate.FlatAppearance.MouseOverBackColor = Color.FromArgb(254, 242, 242);
                btnTerminate.FlatAppearance.MouseDownBackColor = Color.FromArgb(254, 226, 226);
            }

            if (btnCancel.Enabled)
            {
                btnCancel.ForeColor = Color.FromArgb(217, 119, 6);
                btnCancel.FlatAppearance.MouseOverBackColor = Color.FromArgb(254, 243, 199);
                btnCancel.FlatAppearance.MouseDownBackColor = Color.FromArgb(253, 230, 138);
            }
        }

        private bool ConfirmLifecycleAction(string action)
        {
            return MessageBox.Show(
                "Are you sure you want to " + action.ToLowerInvariant() + " this agreement?",
                "Confirm Agreement " + action,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private int? GetSelectedId(ComboBox combo)
        {
            if (combo.SelectedValue == null)
            {
                return null;
            }

            int value;
            return int.TryParse(combo.SelectedValue.ToString(), out value) && value > 0 ? (int?)value : null;
        }

        private string GetSelectedStatusFilter()
        {
            string value = Convert.ToString(cmbFilterStatus.SelectedItem);
            return string.IsNullOrWhiteSpace(value) || value == "All statuses" ? string.Empty : value;
        }

        private static DataTable CreateEmptyTable()
        {
            return new DataTable();
        }

        private static decimal ClampMoney(decimal value, NumericUpDown input)
        {
            if (value < input.Minimum)
            {
                return input.Minimum;
            }

            if (value > input.Maximum)
            {
                return input.Maximum;
            }

            return value;
        }

        private string GenerateAgreementNo(DateTime startDate)
        {
            ServiceResult<string> result = _agreementService.GetNextAgreementNo(startDate);
            return result.IsSuccess ? result.Data : string.Empty;
        }

        private static void AddMissingLookup(List<LookupItem> items, int id, string text)
        {
            if (id <= 0)
            {
                return;
            }

            foreach (LookupItem item in items)
            {
                if (item.Id == id)
                {
                    return;
                }
            }

            items.Add(new LookupItem(id, string.IsNullOrWhiteSpace(text) ? "Selected record" : text + " (current)"));
        }

        private static void SetComboValue(ComboBox combo, int id)
        {
            try
            {
                combo.SelectedValue = id;
            }
            catch
            {
                combo.SelectedIndex = combo.Items.Count > 0 ? 0 : -1;
            }
        }

        private void SetStatus(string message, bool isError)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = isError ? Color.FromArgb(220, 38, 38) : Color.FromArgb(22, 163, 74);
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            StartNewAgreement();
        }

        private void BtnSaveDraft_Click(object sender, EventArgs e)
        {
            SaveDraft();
        }

        private void BtnSaveActive_Click(object sender, EventArgs e)
        {
            SaveActive();
        }

        private void BtnUpdateNotes_Click(object sender, EventArgs e)
        {
            ServiceResult result = _agreementService.UpdateAgreementNotes(_selectedAgreementId, txtNotes.Text);

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                MessageBox.Show(result.Message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetStatus(result.Message, false);
            LoadAgreementRelatedData(_selectedAgreementId);
        }

        private void BtnActivate_Click(object sender, EventArgs e)
        {
            ChangeAgreementLifecycle("Activate");
        }

        private void BtnRenew_Click(object sender, EventArgs e)
        {
            RenewSelectedAgreement();
        }

        private void BtnTerminate_Click(object sender, EventArgs e)
        {
            ChangeAgreementLifecycle("Terminate");
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            ChangeAgreementLifecycle("Cancel");
        }

        private void BtnExpireDue_Click(object sender, EventArgs e)
        {
            ServiceResult result = _agreementService.ExpireDueAgreements();

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                MessageBox.Show(result.Message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetStatus(result.Message, false);
            RefreshAllData();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            RefreshAllData();
            if (_selectedAgreementId > 0)
            {
                LoadAgreementRelatedData(_selectedAgreementId);
            }
        }

        private void DgvAgreements_SelectionChanged(object sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }

            DataRowView view = _agreementBindingSource.Current as DataRowView;
            if (view != null)
            {
                LoadSelectedAgreement(Convert.ToInt32(view["AgreementId"]));
            }
        }

        private void DgvExpiring_SelectionChanged(object sender, EventArgs e)
        {
            DataRowView view = _expiringBindingSource.Current as DataRowView;
            if (view != null)
            {
                SelectAgreementById(Convert.ToInt32(view["AgreementId"]));
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (!_loading)
            {
                LoadAgreements();
            }
        }

        private void CmbFilterStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_loading)
            {
                LoadAgreements();
            }
        }

        private void CmbFilterProperty_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_loading)
            {
                LoadAgreements();
            }
        }

        private void DtpFilter_ValueChanged(object sender, EventArgs e)
        {
            if (!_loading)
            {
                LoadAgreements();
            }
        }

        private void DtpStart_ValueChanged(object sender, EventArgs e)
        {
            if (!_loading && _selectedAgreementId == 0)
            {
                txtAgreementNo.Text = GenerateAgreementNo(dtpStart.Value);
            }
        }

        private void CmbRoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }

            LookupItem item = cmbRoom.SelectedItem as LookupItem;
            if (_selectedAgreementId == 0 && item != null && item.Amount > 0)
            {
                nudMonthlyRent.Value = ClampMoney(item.Amount, nudMonthlyRent);
            }
        }

        private void TabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabMain.SelectedTab == tabExpiring)
            {
                LoadExpiringAgreements();
            }
            else if (_selectedAgreementId > 0 && (tabMain.SelectedTab == tabDetails || tabMain.SelectedTab == tabPayments))
            {
                LoadAgreementRelatedData(_selectedAgreementId);
            }
        }

        private void SplitContainer_Resize(object sender, EventArgs e)
        {
            AdjustSplitter();
        }

        private void AdjustSplitter()
        {
            if (splitContainer == null || splitContainer.Width <= 0)
            {
                return;
            }

            int totalWidth = splitContainer.Width;
            int minimumRequired = splitContainer.Panel1MinSize + splitContainer.Panel2MinSize + splitContainer.SplitterWidth;

            if (totalWidth <= minimumRequired)
            {
                return;
            }

            int editorWidth = totalWidth >= 980
                ? Math.Min(470, Math.Max(390, totalWidth / 3))
                : Math.Max(300, totalWidth / 3);

            int distance = totalWidth - splitContainer.SplitterWidth - editorWidth;
            int minDistance = splitContainer.Panel1MinSize;
            int maxDistance = totalWidth - splitContainer.SplitterWidth - splitContainer.Panel2MinSize;

            if (maxDistance < minDistance)
            {
                return;
            }

            splitContainer.SplitterDistance = Math.Max(minDistance, Math.Min(distance, maxDistance));
        }

        private void Input_Enter(object sender, EventArgs e)
        {
            Control control = sender as Control;
            if (control != null)
            {
                control.BackColor = Color.FromArgb(240, 249, 255);
            }
        }

        private void Input_Leave(object sender, EventArgs e)
        {
            Control control = sender as Control;
            if (control != null)
            {
                control.BackColor = Color.White;
            }
        }

        private void TabMain_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl control = (TabControl)sender;
            if (control.TabPages.Count == 0 || e.Index < 0 || e.Index >= control.TabPages.Count)
            {
                return;
            }

            TabPage page = control.TabPages[e.Index];
            bool selected = control.SelectedIndex == e.Index;
            Color backColor = selected ? Color.White : Color.FromArgb(241, 245, 249);
            Color textColor = selected ? Color.FromArgb(37, 99, 235) : Color.FromArgb(100, 116, 139);

            using (SolidBrush brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            using (Font font = new Font("Segoe UI", 9.5F, selected ? FontStyle.Bold : FontStyle.Regular))
            {
                TextRenderer.DrawText(e.Graphics, page.Text, font, e.Bounds, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            using (Pen pen = new Pen(selected ? Color.FromArgb(37, 99, 235) : Color.FromArgb(226, 232, 240), selected ? 3 : 1))
            {
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 2, e.Bounds.Right, e.Bounds.Bottom - 2);
            }
        }

        private void ResetButtonToSecondary(Button button)
        {
            if (button == null)
            {
                return;
            }

            button.BackColor = Color.FromArgb(241, 245, 249);
            button.ForeColor = Color.FromArgb(51, 65, 85);
            button.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(226, 232, 240);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(203, 213, 225);
        }

        private void HighlightButton(Button button, Color color)
        {
            if (button == null)
            {
                return;
            }

            button.BackColor = color;
            button.ForeColor = Color.White;
            button.FlatAppearance.BorderColor = color;
            button.FlatAppearance.MouseOverBackColor = color;
            button.FlatAppearance.MouseDownBackColor = color;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        private const int EM_SETCUEBANNER = 0x1501;

        private void SetPlaceholder(TextBox textBox, string placeholder)
        {
            if (textBox != null)
            {
                SendMessage(textBox.Handle, EM_SETCUEBANNER, 0, placeholder);
            }
        }

        private class LookupItem
        {
            public LookupItem(int id, string text)
                : this(id, text, 0)
            {
            }

            public LookupItem(int id, string text, decimal amount)
            {
                Id = id;
                Text = text;
                Amount = amount;
            }

            public int Id { get; private set; }
            public string Text { get; private set; }
            public decimal Amount { get; private set; }
        }
    }
}
