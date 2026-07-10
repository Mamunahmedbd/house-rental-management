namespace Housing_rental.Forms.Dashboard
{
    partial class FrmDashboard
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Label lblLogo;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.TableLayoutPanel dashboardGrid;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblCurrentUser;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnDashboard;
        private System.Windows.Forms.Button btnProperties;
        private System.Windows.Forms.Button btnTenants;
        private System.Windows.Forms.Button btnAgreements;
        private System.Windows.Forms.Button btnPayments;
        private System.Windows.Forms.Button btnReports;
        private System.Windows.Forms.Button btnUsers;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Label lblTotalProperties;
        private System.Windows.Forms.Label lblTotalRooms;
        private System.Windows.Forms.Label lblAvailableRooms;
        private System.Windows.Forms.Label lblOccupiedRooms;
        private System.Windows.Forms.Label lblTotalTenants;
        private System.Windows.Forms.Label lblActiveAgreements;
        private System.Windows.Forms.Label lblMonthlyCollected;
        private System.Windows.Forms.Label lblMonthlyDue;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.lblLogo = new System.Windows.Forms.Label();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnUsers = new System.Windows.Forms.Button();
            this.btnReports = new System.Windows.Forms.Button();
            this.btnPayments = new System.Windows.Forms.Button();
            this.btnAgreements = new System.Windows.Forms.Button();
            this.btnTenants = new System.Windows.Forms.Button();
            this.btnProperties = new System.Windows.Forms.Button();
            this.btnDashboard = new System.Windows.Forms.Button();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.btnLogout = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.lblCurrentUser = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.dashboardGrid = new System.Windows.Forms.TableLayoutPanel();
            this.lblTotalProperties = new System.Windows.Forms.Label();
            this.lblTotalRooms = new System.Windows.Forms.Label();
            this.lblAvailableRooms = new System.Windows.Forms.Label();
            this.lblOccupiedRooms = new System.Windows.Forms.Label();
            this.lblTotalTenants = new System.Windows.Forms.Label();
            this.lblActiveAgreements = new System.Windows.Forms.Label();
            this.lblMonthlyCollected = new System.Windows.Forms.Label();
            this.lblMonthlyDue = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.pnlSidebar.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.dashboardGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblLogo
            // 
            this.lblLogo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblLogo.ForeColor = System.Drawing.Color.White;
            this.lblLogo.Location = new System.Drawing.Point(10, 15);
            this.lblLogo.Name = "lblLogo";
            this.lblLogo.Size = new System.Drawing.Size(170, 30);
            this.lblLogo.TabIndex = 8;
            this.lblLogo.Text = "🏠 Rental Admin";
            this.lblLogo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlSidebar
            // 
            this.pnlSidebar.BackColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.pnlSidebar.Controls.Add(this.lblLogo);
            this.pnlSidebar.Controls.Add(this.btnSettings);
            this.pnlSidebar.Controls.Add(this.btnUsers);
            this.pnlSidebar.Controls.Add(this.btnReports);
            this.pnlSidebar.Controls.Add(this.btnPayments);
            this.pnlSidebar.Controls.Add(this.btnAgreements);
            this.pnlSidebar.Controls.Add(this.btnTenants);
            this.pnlSidebar.Controls.Add(this.btnProperties);
            this.pnlSidebar.Controls.Add(this.btnDashboard);
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebar.Location = new System.Drawing.Point(0, 0);
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Size = new System.Drawing.Size(190, 681);
            this.pnlSidebar.TabIndex = 0;
            // 
            // sidebar buttons
            // 
            ConfigureSidebarButton(this.btnDashboard, "Dashboard", 60, true);
            ConfigureSidebarButton(this.btnProperties, "Properties", 108, false);
            ConfigureSidebarButton(this.btnTenants, "Tenants", 156, false);
            ConfigureSidebarButton(this.btnAgreements, "Agreements", 204, false);
            ConfigureSidebarButton(this.btnPayments, "Rent Collection", 252, false);
            ConfigureSidebarButton(this.btnReports, "Reports", 300, false);
            ConfigureSidebarButton(this.btnUsers, "Users", 348, false);
            ConfigureSidebarButton(this.btnSettings, "Settings", 396, false);
            this.btnDashboard.Click += new System.EventHandler(this.BtnDashboard_Click);
            this.btnProperties.Click += new System.EventHandler(this.BtnProperties_Click);
            this.btnTenants.Click += new System.EventHandler(this.BtnTenants_Click);
            this.btnAgreements.Click += new System.EventHandler(this.BtnAgreements_Click);
            this.btnPayments.Click += new System.EventHandler(this.BtnPayments_Click);
            this.btnReports.Click += new System.EventHandler(this.BtnReports_Click);
            this.btnUsers.Click += new System.EventHandler(this.BtnUsers_Click);
            this.btnSettings.Click += new System.EventHandler(this.BtnSettings_Click);
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.White;
            this.pnlHeader.Controls.Add(this.btnLogout);
            this.pnlHeader.Controls.Add(this.btnRefresh);
            this.pnlHeader.Controls.Add(this.lblCurrentUser);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(190, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(944, 88);
            this.pnlHeader.TabIndex = 1;
            // 
            // pnlContent
            // 
            this.pnlContent.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.pnlContent.Controls.Add(this.lblStatus);
            this.pnlContent.Controls.Add(this.dashboardGrid);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(190, 88);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Size = new System.Drawing.Size(944, 593);
            this.pnlContent.TabIndex = 2;
            // 
            // btnLogout
            // 
            this.btnLogout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLogout.Location = new System.Drawing.Point(832, 28);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(88, 34);
            this.btnLogout.TabIndex = 3;
            this.btnLogout.Text = "Logout";
            this.btnLogout.UseVisualStyleBackColor = true;
            this.btnLogout.Click += new System.EventHandler(this.BtnLogout_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.Location = new System.Drawing.Point(738, 28);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(88, 34);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            // 
            // lblCurrentUser
            // 
            this.lblCurrentUser.AutoSize = true;
            this.lblCurrentUser.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCurrentUser.ForeColor = System.Drawing.Color.DimGray;
            this.lblCurrentUser.Location = new System.Drawing.Point(24, 55);
            this.lblCurrentUser.Name = "lblCurrentUser";
            this.lblCurrentUser.Size = new System.Drawing.Size(35, 15);
            this.lblCurrentUser.TabIndex = 1;
            this.lblCurrentUser.Text = "User:";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(21, 18);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(140, 32);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Dashboard";
            // 
            // dashboardGrid
            // 
            this.dashboardGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dashboardGrid.ColumnCount = 4;
            this.dashboardGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.dashboardGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.dashboardGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.dashboardGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.dashboardGrid.Controls.Add(this.lblTotalProperties, 0, 0);
            this.dashboardGrid.Controls.Add(this.lblTotalRooms, 1, 0);
            this.dashboardGrid.Controls.Add(this.lblAvailableRooms, 2, 0);
            this.dashboardGrid.Controls.Add(this.lblOccupiedRooms, 3, 0);
            this.dashboardGrid.Controls.Add(this.lblTotalTenants, 0, 1);
            this.dashboardGrid.Controls.Add(this.lblActiveAgreements, 1, 1);
            this.dashboardGrid.Controls.Add(this.lblMonthlyCollected, 2, 1);
            this.dashboardGrid.Controls.Add(this.lblMonthlyDue, 3, 1);
            this.dashboardGrid.Location = new System.Drawing.Point(28, 40);
            this.dashboardGrid.Name = "dashboardGrid";
            this.dashboardGrid.RowCount = 2;
            this.dashboardGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.dashboardGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.dashboardGrid.Size = new System.Drawing.Size(884, 300);
            this.dashboardGrid.TabIndex = 2;
            // 
            // summary labels
            // 
            ConfigureSummaryLabel(this.lblTotalProperties, "0\nTotal Properties");
            ConfigureSummaryLabel(this.lblTotalRooms, "0\nTotal Rooms");
            ConfigureSummaryLabel(this.lblAvailableRooms, "0\nAvailable Rooms");
            ConfigureSummaryLabel(this.lblOccupiedRooms, "0\nOccupied Rooms");
            ConfigureSummaryLabel(this.lblTotalTenants, "0\nTotal Tenants");
            ConfigureSummaryLabel(this.lblActiveAgreements, "0\nActive Agreements");
            ConfigureSummaryLabel(this.lblMonthlyCollected, "$0.00\nMonthly Collected");
            ConfigureSummaryLabel(this.lblMonthlyDue, "$0.00\nMonthly Due");
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatus.ForeColor = System.Drawing.Color.DimGray;
            this.lblStatus.Location = new System.Drawing.Point(28, 552);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(40, 15);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "Ready.";
            // 
            // FrmDashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.ClientSize = new System.Drawing.Size(1134, 681);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlSidebar);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(980, 620);
            this.Name = "FrmDashboard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Dashboard - House Rental Management System";
            this.Load += new System.EventHandler(this.FrmDashboard_Load);
            this.pnlSidebar.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlContent.ResumeLayout(false);
            this.pnlContent.PerformLayout();
            this.dashboardGrid.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ConfigureSidebarButton(System.Windows.Forms.Button button, string text, int top, bool selected)
        {
            button.BackColor = System.Drawing.Color.Transparent;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(51, 65, 85);
            button.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(15, 23, 42);
            button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            button.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            button.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184);
            button.Location = new System.Drawing.Point(0, top);
            button.Name = "btn" + text.Replace(" ", string.Empty);
            button.Size = new System.Drawing.Size(190, 48);
            button.Text = "   " + text;
            button.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            button.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            button.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            button.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            button.UseVisualStyleBackColor = false;
            button.Paint += new System.Windows.Forms.PaintEventHandler(this.SidebarButton_Paint);
        }

        private void ConfigureSummaryLabel(System.Windows.Forms.Label label, string text)
        {
            label.BackColor = System.Drawing.Color.White;
            label.Dock = System.Windows.Forms.DockStyle.Fill;
            label.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            label.ForeColor = System.Drawing.Color.FromArgb(37, 52, 73);
            label.Margin = new System.Windows.Forms.Padding(10);
            label.Text = text;
            label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        }
    }
}
