using System;
using System.Drawing;
using System.Windows.Forms;
using Housing_rental.BLL;
using Housing_rental.Forms.Admin;
using Housing_rental.Forms.Common;
using Housing_rental.Models;

namespace Housing_rental.Forms.Dashboard
{
    public partial class FrmDashboard : Form
    {
        private readonly DashboardService _dashboardService;

        public FrmDashboard()
        {
            InitializeComponent();
            _dashboardService = new DashboardService();
        }

        private void FrmDashboard_Load(object sender, EventArgs e)
        {
            lblCurrentUser.Text = "User: " + CurrentSession.User.FullName + " (" + CurrentSession.User.RoleName + ")";
            btnUsers.Visible = CurrentSession.IsAdmin;
            btnSettings.Visible = CurrentSession.IsAdmin;
            ShowDashboard();
            LoadDashboardSummary();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            if (dashboardGrid.Visible)
            {
                LoadDashboardSummary();
            }
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnProperties_Click(object sender, EventArgs e)
        {
            ShowModule("Property, House, and Room Management", "This module will manage the Property -> House -> Room hierarchy, room rent, availability, occupancy, and maintenance status.");
        }

        private void BtnTenants_Click(object sender, EventArgs e)
        {
            ShowModule("Tenant Management", "This module will manage tenant profiles, contact information, emergency contacts, agreement history, and payment history.");
        }

        private void BtnAgreements_Click(object sender, EventArgs e)
        {
            ShowModule("Rental Agreement Management", "This module will create, renew, terminate, and track rental agreements between tenants and available rooms.");
        }

        private void BtnPayments_Click(object sender, EventArgs e)
        {
            ShowModule("Monthly Rent Collection", "This module will collect monthly rent, calculate balances, track payment status, and prepare receipt data.");
        }

        private void BtnReports_Click(object sender, EventArgs e)
        {
            ShowModule("Reports", "This module will show RDLC reports for tenants, occupancy, agreements, rent collection, monthly dues, and income summary.");
        }

        private void BtnUsers_Click(object sender, EventArgs e)
        {
            NavigateToControl("User Management", new UserManagementControl());
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            ShowModule("Settings", "This Admin module will manage application settings such as currency, receipt footer, and rent due day.");
        }

        private void ShowModule(string moduleName, string description)
        {
            NavigateToControl(moduleName, new ModulePlaceholderControl(moduleName, description));
        }

        private void BtnDashboard_Click(object sender, EventArgs e)
        {
            ShowDashboard();
            LoadDashboardSummary();
        }

        private void ShowDashboard()
        {
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(lblStatus);
            pnlContent.Controls.Add(dashboardGrid);
            dashboardGrid.Visible = true;
            lblStatus.Visible = true;
            lblTitle.Text = "Dashboard";
            btnRefresh.Enabled = true;
        }

        private void NavigateToControl(string title, Control control)
        {
            pnlContent.Controls.Clear();
            control.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(control);
            lblTitle.Text = title;
            btnRefresh.Enabled = false;
        }

        private void LoadDashboardSummary()
        {
            ServiceResult<DashboardSummary> result = _dashboardService.GetSummary();

            if (!result.IsSuccess)
            {
                lblStatus.Text = result.Message;
                lblStatus.ForeColor = Color.Firebrick;
                return;
            }

            DashboardSummary summary = result.Data;
            lblTotalProperties.Text = summary.TotalProperties + "\nTotal Properties";
            lblTotalRooms.Text = summary.TotalRooms + "\nTotal Rooms";
            lblAvailableRooms.Text = summary.AvailableRooms + "\nAvailable Rooms";
            lblOccupiedRooms.Text = summary.OccupiedRooms + "\nOccupied Rooms";
            lblTotalTenants.Text = summary.TotalTenants + "\nTotal Tenants";
            lblActiveAgreements.Text = summary.ActiveAgreements + "\nActive Agreements";
            lblMonthlyCollected.Text = summary.MonthlyCollectedRent.ToString("C") + "\nMonthly Collected";
            lblMonthlyDue.Text = summary.MonthlyDueRent.ToString("C") + "\nMonthly Due";

            lblStatus.Text = "Dashboard loaded successfully.";
            lblStatus.ForeColor = Color.ForestGreen;
        }
    }
}
