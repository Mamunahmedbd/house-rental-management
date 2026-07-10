using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Housing_rental.BLL;
using Housing_rental.Models;

namespace Housing_rental.Forms.Tenants
{
    public partial class TenantManagementControl : UserControl
    {
        private readonly TenantService _tenantService;
        private readonly BindingSource _tenantBindingSource;
        private readonly BindingSource _currentOccupancyBindingSource;
        private readonly BindingSource _balanceBindingSource;
        private readonly BindingSource _agreementHistoryBindingSource;
        private readonly BindingSource _paymentHistoryBindingSource;

        private bool _loading;
        private int _selectedTenantId;

        public TenantManagementControl()
        {
            _tenantService = new TenantService();
            _tenantBindingSource = new BindingSource();
            _currentOccupancyBindingSource = new BindingSource();
            _balanceBindingSource = new BindingSource();
            _agreementHistoryBindingSource = new BindingSource();
            _paymentHistoryBindingSource = new BindingSource();

            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _loading = true;
            cmbFilterStatus.SelectedIndex = 0;
            cmbTenantStatus.SelectedItem = "Active";
            _loading = false;

            SetPlaceholder(txtSearch, "Search tenants by name, phone, email, national ID or address...");
            LoadTenants();
            StartNewTenant();
            AdjustSplitter();
        }

        private void LoadTenants()
        {
            ServiceResult<List<Tenant>> result = _tenantService.SearchTenants(
                txtSearch == null ? string.Empty : txtSearch.Text,
                GetSelectedStatusFilter(),
                chkIncludeInactive.Checked);

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                return;
            }

            _tenantBindingSource.DataSource = result.Data;
            SetStatus(result.Data.Count == 0 ? "No tenants found for the selected filters." : result.Message, false);
        }

        private void LoadTenantRelatedData(int tenantId)
        {
            if (tenantId <= 0)
            {
                _currentOccupancyBindingSource.DataSource = CreateEmptyTable();
                _balanceBindingSource.DataSource = CreateEmptyTable();
                _agreementHistoryBindingSource.DataSource = CreateEmptyTable();
                _paymentHistoryBindingSource.DataSource = CreateEmptyTable();
                return;
            }

            LoadCurrentOccupancy(tenantId);
            LoadBalanceSummary(tenantId);
            LoadAgreementHistory(tenantId);
            LoadPaymentHistory(tenantId);
        }

        private void LoadCurrentOccupancy(int tenantId)
        {
            ServiceResult<DataTable> result = _tenantService.GetTenantCurrentOccupancy(tenantId);

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                return;
            }

            _currentOccupancyBindingSource.DataSource = result.Data;
        }

        private void LoadBalanceSummary(int tenantId)
        {
            ServiceResult<DataTable> result = _tenantService.GetTenantBalanceSummary(tenantId);

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                return;
            }

            _balanceBindingSource.DataSource = result.Data;
        }

        private void LoadAgreementHistory(int tenantId)
        {
            ServiceResult<DataTable> result = _tenantService.GetTenantAgreementHistory(tenantId);

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                return;
            }

            _agreementHistoryBindingSource.DataSource = result.Data;
        }

        private void LoadPaymentHistory(int tenantId)
        {
            ServiceResult<DataTable> result = _tenantService.GetTenantPaymentHistory(tenantId);

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                return;
            }

            _paymentHistoryBindingSource.DataSource = result.Data;
        }

        private void StartNewTenant()
        {
            _selectedTenantId = 0;
            lblMode.Text = "Create Tenant";
            btnSave.Text = "Create Tenant";
            txtFullName.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtNationalId.Clear();
            txtAddress.Clear();
            txtEmergencyContactName.Clear();
            txtEmergencyContactPhone.Clear();
            cmbTenantStatus.SelectedItem = "Active";
            btnToggleStatus.Enabled = false;
            btnBlacklist.Enabled = false;
            ResetButtonToSecondary(btnToggleStatus);
            ResetButtonToSecondary(btnBlacklist);
            LoadTenantRelatedData(0);
            txtFullName.Focus();
        }

        private void LoadSelectedTenant(Tenant tenant)
        {
            if (tenant == null)
            {
                return;
            }

            _selectedTenantId = tenant.TenantId;
            lblMode.Text = "Edit Tenant";
            btnSave.Text = "Save Tenant";
            txtFullName.Text = tenant.FullName;
            txtPhone.Text = tenant.Phone;
            txtEmail.Text = tenant.Email;
            txtNationalId.Text = tenant.NationalId;
            txtAddress.Text = tenant.Address;
            txtEmergencyContactName.Text = tenant.EmergencyContactName;
            txtEmergencyContactPhone.Text = tenant.EmergencyContactPhone;
            cmbTenantStatus.SelectedItem = tenant.Status;
            UpdateStatusButtons(tenant.Status);
            LoadTenantRelatedData(tenant.TenantId);
        }

        private Tenant ReadTenantFromForm()
        {
            return new Tenant
            {
                TenantId = _selectedTenantId,
                FullName = txtFullName.Text,
                Phone = txtPhone.Text,
                Email = txtEmail.Text,
                NationalId = txtNationalId.Text,
                Address = txtAddress.Text,
                EmergencyContactName = txtEmergencyContactName.Text,
                EmergencyContactPhone = txtEmergencyContactPhone.Text,
                Status = Convert.ToString(cmbTenantStatus.SelectedItem)
            };
        }

        private void SaveTenant()
        {
            Tenant tenant = ReadTenantFromForm();
            ServiceResult result = _selectedTenantId == 0
                ? _tenantService.CreateTenant(tenant)
                : _tenantService.UpdateTenant(tenant);

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                MessageBox.Show(result.Message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetStatus(result.Message, false);
            LoadTenants();

            int tenantId = _selectedTenantId > 0 ? _selectedTenantId : FindTenantId(tenant);
            if (tenantId > 0)
            {
                SelectTenantById(tenantId);
            }
        }

        private int FindTenantId(Tenant candidate)
        {
            foreach (Tenant tenant in _tenantBindingSource.List)
            {
                if (!string.IsNullOrWhiteSpace(candidate.NationalId)
                    && string.Equals(tenant.NationalId, candidate.NationalId, StringComparison.OrdinalIgnoreCase))
                {
                    return tenant.TenantId;
                }

                if (string.Equals(tenant.FullName, candidate.FullName, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(tenant.Phone, candidate.Phone, StringComparison.OrdinalIgnoreCase))
                {
                    return tenant.TenantId;
                }
            }

            return 0;
        }

        private void SelectTenantById(int tenantId)
        {
            foreach (DataGridViewRow row in dgvTenants.Rows)
            {
                Tenant tenant = row.DataBoundItem as Tenant;

                if (tenant != null && tenant.TenantId == tenantId)
                {
                    row.Selected = true;
                    dgvTenants.CurrentCell = row.Cells[0];
                    LoadSelectedTenant(tenant);
                    return;
                }
            }
        }

        private void ChangeSelectedTenantStatus(string status)
        {
            Tenant tenant = _tenantBindingSource.Current as Tenant;

            if (tenant == null)
            {
                SetStatus("Please select a tenant first.", true);
                return;
            }

            if (!ConfirmStatusChange(tenant, status))
            {
                return;
            }

            ServiceResult result = _tenantService.SetTenantStatus(tenant.TenantId, status);

            if (!result.IsSuccess)
            {
                SetStatus(result.Message, true);
                MessageBox.Show(result.Message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetStatus(result.Message, false);
            LoadTenants();
            SelectTenantById(tenant.TenantId);
        }

        private bool ConfirmStatusChange(Tenant tenant, string status)
        {
            string action = status == "Active"
                ? "activate"
                : status == "Blacklisted" ? "blacklist" : "deactivate";

            return MessageBox.Show(
                "Are you sure you want to " + action + " tenant '" + tenant.FullName + "'?",
                "Confirm Tenant Status",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private void UpdateStatusButtons(string status)
        {
            btnToggleStatus.Enabled = true;
            btnBlacklist.Enabled = true;
            ResetButtonToSecondary(btnToggleStatus);
            ResetButtonToSecondary(btnBlacklist);

            if (status == "Active")
            {
                btnToggleStatus.Text = "Deactivate";
                btnBlacklist.Text = "Blacklist";
                
                // Red theme for Deactivate
                btnToggleStatus.ForeColor = Color.FromArgb(220, 38, 38);
                btnToggleStatus.FlatAppearance.MouseOverBackColor = Color.FromArgb(254, 242, 242);
                btnToggleStatus.FlatAppearance.MouseDownBackColor = Color.FromArgb(254, 226, 226);
                
                // Amber theme for Blacklist
                btnBlacklist.ForeColor = Color.FromArgb(217, 119, 6);
                btnBlacklist.FlatAppearance.MouseOverBackColor = Color.FromArgb(254, 243, 199);
                btnBlacklist.FlatAppearance.MouseDownBackColor = Color.FromArgb(253, 230, 138);
                btnBlacklist.Enabled = CurrentSession.IsAdmin;
            }
            else if (status == "Inactive")
            {
                btnToggleStatus.Text = "Activate";
                btnBlacklist.Text = "Blacklist";
                
                // Green theme for Activate
                btnToggleStatus.ForeColor = Color.FromArgb(22, 163, 74);
                btnToggleStatus.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 253, 250);
                btnToggleStatus.FlatAppearance.MouseDownBackColor = Color.FromArgb(204, 251, 241);
                
                // Amber theme for Blacklist
                btnBlacklist.ForeColor = Color.FromArgb(217, 119, 6);
                btnBlacklist.FlatAppearance.MouseOverBackColor = Color.FromArgb(254, 243, 199);
                btnBlacklist.FlatAppearance.MouseDownBackColor = Color.FromArgb(253, 230, 138);
                btnBlacklist.Enabled = CurrentSession.IsAdmin;
            }
            else
            {
                btnToggleStatus.Text = "Reactivate";
                btnBlacklist.Text = "Blacklisted";
                
                // Green theme for Reactivate
                btnToggleStatus.ForeColor = Color.FromArgb(22, 163, 74);
                btnToggleStatus.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 253, 250);
                btnToggleStatus.FlatAppearance.MouseDownBackColor = Color.FromArgb(204, 251, 241);
                
                // Solid highlight red for Blacklisted warning indicator
                btnBlacklist.BackColor = Color.FromArgb(220, 38, 38);
                btnBlacklist.ForeColor = Color.White;
                btnBlacklist.FlatAppearance.BorderColor = Color.FromArgb(220, 38, 38);
                btnBlacklist.Enabled = false;
            }
        }

        private void RefreshActiveTab()
        {
            if (_loading)
            {
                return;
            }

            if (tabMain.SelectedTab == tabTenants)
            {
                LoadTenants();
            }
            else if (_selectedTenantId > 0)
            {
                LoadTenantRelatedData(_selectedTenantId);
            }
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

        private void SetStatus(string message, bool isError)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = isError ? Color.FromArgb(220, 38, 38) : Color.FromArgb(22, 163, 74);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveTenant();
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            StartNewTenant();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadTenants();
            if (_selectedTenantId > 0)
            {
                LoadTenantRelatedData(_selectedTenantId);
            }
        }

        private void BtnToggleStatus_Click(object sender, EventArgs e)
        {
            Tenant tenant = _tenantBindingSource.Current as Tenant;
            if (tenant == null)
            {
                SetStatus("Please select a tenant first.", true);
                return;
            }

            ChangeSelectedTenantStatus(tenant.Status == "Active" ? "Inactive" : "Active");
        }

        private void BtnBlacklist_Click(object sender, EventArgs e)
        {
            ChangeSelectedTenantStatus("Blacklisted");
        }

        private void DgvTenants_SelectionChanged(object sender, EventArgs e)
        {
            LoadSelectedTenant(_tenantBindingSource.Current as Tenant);
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadTenants();
        }

        private void CmbFilterStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshActiveTab();
        }

        private void ChkIncludeInactive_CheckedChanged(object sender, EventArgs e)
        {
            LoadTenants();
        }

        private void TabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshActiveTab();
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
                ? Math.Min(460, Math.Max(380, totalWidth / 3))
                : Math.Max(280, totalWidth / 3);

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
    }
}
