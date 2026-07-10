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
        private Button[] _sidebarButtons;
        private Button _activeSidebarButton;

        public event EventHandler LogoutRequested;

        public FrmDashboard()
        {
            InitializeComponent();
            _dashboardService = new DashboardService();
        }

        private void FrmDashboard_Load(object sender, EventArgs e)
        {
            _sidebarButtons = new Button[] { btnDashboard, btnProperties, btnTenants, btnAgreements, btnPayments, btnReports, btnUsers, btnSettings };

            lblCurrentUser.Text = "User: " + CurrentSession.User.FullName + " (" + CurrentSession.User.RoleName + ")";
            btnUsers.Visible = CurrentSession.IsAdmin;
            btnSettings.Visible = CurrentSession.IsAdmin;

            SetActiveButton(btnDashboard);
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
            DialogResult result = MessageBox.Show(
                "Do you want to sign out and return to the login screen?",
                "Confirm Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            OnLogoutRequested();
        }

        private void BtnProperties_Click(object sender, EventArgs e)
        {
            SetActiveButton(sender as Button);
            ShowModule("Property, House, and Room Management", "This module will manage the Property -> House -> Room hierarchy, room rent, availability, occupancy, and maintenance status.");
        }

        private void BtnTenants_Click(object sender, EventArgs e)
        {
            SetActiveButton(sender as Button);
            ShowModule("Tenant Management", "This module will manage tenant profiles, contact information, emergency contacts, agreement history, and payment history.");
        }

        private void BtnAgreements_Click(object sender, EventArgs e)
        {
            SetActiveButton(sender as Button);
            ShowModule("Rental Agreement Management", "This module will create, renew, terminate, and track rental agreements between tenants and available rooms.");
        }

        private void BtnPayments_Click(object sender, EventArgs e)
        {
            SetActiveButton(sender as Button);
            ShowModule("Monthly Rent Collection", "This module will collect monthly rent, calculate balances, track payment status, and prepare receipt data.");
        }

        private void BtnReports_Click(object sender, EventArgs e)
        {
            SetActiveButton(sender as Button);
            ShowModule("Reports", "This module will show RDLC reports for tenants, occupancy, agreements, rent collection, monthly dues, and income summary.");
        }

        private void BtnUsers_Click(object sender, EventArgs e)
        {
            SetActiveButton(sender as Button);
            NavigateToControl("User Management", new UserManagementControl());
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            SetActiveButton(sender as Button);
            ShowModule("Settings", "This Admin module will manage application settings such as currency, receipt footer, and rent due day.");
        }

        private void ShowModule(string moduleName, string description)
        {
            NavigateToControl(moduleName, new ModulePlaceholderControl(moduleName, description));
        }

        private void BtnDashboard_Click(object sender, EventArgs e)
        {
            SetActiveButton(sender as Button);
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

        private void OnLogoutRequested()
        {
            EventHandler handler = LogoutRequested;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private Image CreateIcon(string glyph, Color color, int size = 16)
        {
            Bitmap bmp = new Bitmap(size + 8, size + 8);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                using (Font font = new Font("Segoe MDL2 Assets", size, FontStyle.Regular))
                using (Brush brush = new SolidBrush(color))
                {
                    SizeF glyphSize = g.MeasureString(glyph, font);
                    float x = (bmp.Width - glyphSize.Width) / 2f;
                    float y = (bmp.Height - glyphSize.Height) / 2f;
                    g.DrawString(glyph, font, brush, x, y);
                }
            }
            return bmp;
        }

        private string GetButtonGlyph(Button button)
        {
            if (button == btnDashboard) return "\uE9D9";
            if (button == btnProperties) return "\uEA49";
            if (button == btnTenants) return "\uE716";
            if (button == btnAgreements) return "\uE8A5";
            if (button == btnPayments) return "\uE94C";
            if (button == btnReports) return "\uE9D2";
            if (button == btnUsers) return "\uE77B";
            if (button == btnSettings) return "\uE713";
            return "";
        }

        private void UpdateButtonIcon(Button btn, bool isActive)
        {
            string glyph = GetButtonGlyph(btn);
            if (!string.IsNullOrEmpty(glyph))
            {
                Color iconColor = isActive ? Color.FromArgb(248, 250, 252) : Color.FromArgb(148, 163, 184);
                if (btn.Image != null)
                {
                    btn.Image.Dispose();
                }
                btn.Image = CreateIcon(glyph, iconColor, 16);
            }
        }

        private void SetActiveButton(Button button)
        {
            if (button == null) return;
            _activeSidebarButton = button;

            foreach (var btn in _sidebarButtons)
            {
                if (btn == null) continue;
                bool isActive = (btn == button);

                btn.BackColor = isActive ? Color.FromArgb(15, 23, 42) : Color.Transparent;
                btn.Font = new Font("Segoe UI", 10F, isActive ? FontStyle.Bold : FontStyle.Regular);
                btn.ForeColor = isActive ? Color.FromArgb(248, 250, 252) : Color.FromArgb(148, 163, 184);

                UpdateButtonIcon(btn, isActive);
                btn.Invalidate();
            }
        }

        private void SidebarButton_Paint(object sender, PaintEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            bool isActive = (btn == _activeSidebarButton);
            if (isActive)
            {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(59, 130, 246)))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, 4, btn.Height);
                }
            }
        }
    }
}
