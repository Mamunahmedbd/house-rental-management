using System;
using System.Drawing;
using System.Windows.Forms;

namespace Housing_rental.Forms.Reports
{
    partial class ReportManagementControl
    {
        private System.ComponentModel.IContainer components = null;
        private ComboBox cboReportType;
        private FlowLayoutPanel pnlFiltersFlow;
        
        // Filter blocks (for grouped flow control)
        private Control blockReportType;
        private Control blockStatus;
        private Control blockProperty;
        private Control blockDateFrom;
        private Control blockDateTo;
        private Control blockBillingPeriod;
        private Control blockSearch;
        private Control blockIncludeInactive;
        private Control blockButtons;

        // Individual controls
        private ComboBox cboStatusFilter;
        private ComboBox cboPropertyFilter;
        private DateTimePicker dtpDateFrom;
        private DateTimePicker dtpDateTo;
        private DateTimePicker dtpBillingPeriod;
        private TextBox txtSearch;
        private CheckBox chkIncludeInactive;
        private Button btnGenerate;
        private Button btnClear;
        private Microsoft.Reporting.WinForms.ReportViewer reportViewer;
        private Label lblStatus;

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
            SuspendLayout();

            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(248, 250, 252);
            Font = new Font("Segoe UI", 9F);
            Padding = new Padding(18);
            Size = new Size(1000, 650);

            TableLayoutPanel root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = BackColor,
                ColumnCount = 1,
                RowCount = 3
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));  // Height adjusted for vertical block layout
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // ReportViewer height
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));   // Status bar height

            // Filter Flow Layout Panel (will hold all aligned filter blocks)
            pnlFiltersFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(12, 10, 12, 10),
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 12)
            };
            pnlFiltersFlow.Paint += Panel_Paint;
            root.Controls.Add(pnlFiltersFlow, 0, 0);

            // Initialize controls
            cboReportType = CreateCombo(190);
            cboStatusFilter = CreateCombo(120);
            cboPropertyFilter = CreateCombo(160);
            
            dtpDateFrom = CreateDatePicker(110);
            dtpDateTo = CreateDatePicker(110);
            dtpBillingPeriod = CreateDatePicker(130);
            dtpBillingPeriod.CustomFormat = "MMMM yyyy";
            dtpBillingPeriod.Format = DateTimePickerFormat.Custom;
            dtpBillingPeriod.ShowUpDown = true;

            txtSearch = CreateInput(140);

            chkIncludeInactive = new CheckBox
            {
                Text = "Include Inactive Rooms",
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                AutoSize = true,
                Location = new Point(0, 26) // Align with input baseline
            };

            btnGenerate = CreatePrimaryButton("Generate Report");
            btnGenerate.AutoSize = false;
            btnGenerate.Width = 125;
            btnGenerate.Click += BtnGenerate_Click;
            
            btnClear = CreateSecondaryButton("Clear");
            btnClear.AutoSize = false;
            btnClear.Width = 80;
            btnClear.Click += BtnClear_Click;

            // Build aligned blocks
            blockReportType = CreateFieldBlock("Report Type", cboReportType, 190);
            blockStatus = CreateFieldBlock("Status", cboStatusFilter, 120);
            blockProperty = CreateFieldBlock("Property", cboPropertyFilter, 160);
            blockDateFrom = CreateFieldBlock("From Date", dtpDateFrom, 110);
            blockDateTo = CreateFieldBlock("To Date", dtpDateTo, 110);
            blockBillingPeriod = CreateFieldBlock("Billing Month", dtpBillingPeriod, 130);
            blockSearch = CreateFieldBlock("Search", txtSearch, 140);
            
            // Special block for checkbox to align with input baseline
            blockIncludeInactive = new Panel { Height = 62, Width = 170, Margin = new Padding(0, 0, 8, 0) };
            blockIncludeInactive.Controls.Add(chkIncludeInactive);

            // Special block for buttons to align with input baseline
            blockButtons = new Panel { Height = 62, Width = 225, Margin = new Padding(8, 0, 0, 0) };
            btnGenerate.Location = new Point(0, 21); // Aligns with the 22px Y coordinate baseline
            btnClear.Location = new Point(133, 21); // Aligns with fixed spacing (125 width + 8 spacing = 133)
            blockButtons.Controls.Add(btnGenerate);
            blockButtons.Controls.Add(btnClear);

            // Add all blocks to flow layout
            pnlFiltersFlow.Controls.Add(blockReportType);
            pnlFiltersFlow.Controls.Add(blockStatus);
            pnlFiltersFlow.Controls.Add(blockProperty);
            pnlFiltersFlow.Controls.Add(blockDateFrom);
            pnlFiltersFlow.Controls.Add(blockDateTo);
            pnlFiltersFlow.Controls.Add(blockBillingPeriod);
            pnlFiltersFlow.Controls.Add(blockSearch);
            pnlFiltersFlow.Controls.Add(blockIncludeInactive);
            pnlFiltersFlow.Controls.Add(blockButtons);

            // Report Viewer
            reportViewer = new Microsoft.Reporting.WinForms.ReportViewer
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None
            };
            
            // Clean panel outline for report viewer
            Panel viewerContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(1),
                Margin = new Padding(0, 0, 0, 10)
            };
            viewerContainer.Paint += Panel_Paint;
            viewerContainer.Controls.Add(reportViewer);
            root.Controls.Add(viewerContainer, 0, 1);

            // Status Label
            lblStatus = new Label
            {
                AutoEllipsis = true,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(100, 116, 139),
                Text = "Ready.",
                TextAlign = ContentAlignment.MiddleLeft
            };
            root.Controls.Add(lblStatus, 0, 2);

            Controls.Add(root);
            Load += ReportManagementControl_Load;
            ResumeLayout(false);
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

        private static TextBox CreateInput(int width)
        {
            return new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0),
                Width = width
            };
        }

        private static ComboBox CreateCombo(int width)
        {
            return new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0),
                Width = width
            };
        }

        private static DateTimePicker CreateDatePicker(int width)
        {
            return new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0),
                Width = width
            };
        }

        private static Button CreatePrimaryButton(string text)
        {
            Button button = CreateBaseButton(text);
            button.BackColor = Color.FromArgb(37, 99, 235);
            button.ForeColor = Color.White;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(29, 78, 216);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 64, 175);
            return button;
        }

        private static Button CreateSecondaryButton(string text)
        {
            Button button = CreateBaseButton(text);
            button.BackColor = Color.FromArgb(241, 245, 249);
            button.ForeColor = Color.FromArgb(51, 65, 85);
            button.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(203, 213, 225);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(226, 232, 240);
            return button;
        }

        private static Button CreateBaseButton(string text)
        {
            Button button = new Button
            {
                Height = 28,
                Text = text,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                UseVisualStyleBackColor = false,
                AutoSize = true,
                Padding = new Padding(12, 0, 12, 0)
            };
            button.FlatAppearance.BorderSize = 1;
            return button;
        }

        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, ((Panel)sender).ClientRectangle,
                Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
        }
    }
}
