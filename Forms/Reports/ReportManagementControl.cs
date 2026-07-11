using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Housing_rental.BLL;
using Housing_rental.Models;
using Microsoft.Reporting.WinForms;

namespace Housing_rental.Forms.Reports
{
    public partial class ReportManagementControl : UserControl
    {
        private readonly ReportService _reportService;
        private List<ReportDefinition> _reports;
        private bool _loading;

        public ReportManagementControl()
        {
            _reportService = new ReportService();
            _loading = true;
            InitializeComponent();
        }

        private void ReportManagementControl_Load(object sender, EventArgs e)
        {
            LoadReportDefinitions();
            LoadPropertyFilter();
            ResetFilters();
            
            _loading = false;
            
            // Trigger initial visibility
            CboReportType_SelectedIndexChanged(this, EventArgs.Empty);
        }

        private void LoadReportDefinitions()
        {
            _reports = _reportService.GetReportDefinitions();
            cboReportType.DataSource = _reports;
            cboReportType.DisplayMember = "DisplayName";
            cboReportType.ValueMember = "ReportType";
        }

        private void LoadPropertyFilter()
        {
            ServiceResult<DataTable> result = _reportService.GetActiveProperties();
            if (!result.IsSuccess)
            {
                ShowStatus("Error loading properties: " + result.Message, false);
                return;
            }

            DataTable dt = result.Data.Copy();
            
            // Insert default row for "All Properties"
            DataRow row = dt.NewRow();
            row["PropertyId"] = DBNull.Value;
            row["PropertyName"] = "All Properties";
            dt.Rows.InsertAt(row, 0);

            cboPropertyFilter.DataSource = dt;
            cboPropertyFilter.DisplayMember = "PropertyName";
            cboPropertyFilter.ValueMember = "PropertyId";
        }

        private void ResetFilters()
        {
            dtpDateFrom.Value = DateTime.Today.AddMonths(-3);
            dtpDateTo.Value = DateTime.Today;
            dtpBillingPeriod.Value = DateTime.Today;
            txtSearch.Text = string.Empty;
            chkIncludeInactive.Checked = false;

            if (cboPropertyFilter.Items.Count > 0)
                cboPropertyFilter.SelectedIndex = 0;
        }

        private void CboReportType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading || cboReportType.SelectedValue == null)
                return;

            string reportType = cboReportType.SelectedValue.ToString();
            UpdateFilterVisibility(reportType);
        }

        private void UpdateFilterVisibility(string reportType)
        {
            pnlFiltersFlow.SuspendLayout();

            // Clear previous bindings or items for Status ComboBox
            cboStatusFilter.Items.Clear();

            switch (reportType)
            {
                case ReportTypes.TenantList:
                    blockStatus.Visible = true;
                    blockProperty.Visible = false;
                    blockDateFrom.Visible = false;
                    blockDateTo.Visible = false;
                    blockBillingPeriod.Visible = false;
                    blockSearch.Visible = true;
                    blockIncludeInactive.Visible = false;
                    
                    cboStatusFilter.Items.AddRange(new object[] { "All", "Active", "Inactive", "Blacklisted" });
                    cboStatusFilter.SelectedIndex = 0;
                    break;

                case ReportTypes.PropertyOccupancy:
                    blockStatus.Visible = false;
                    blockProperty.Visible = true;
                    blockDateFrom.Visible = false;
                    blockDateTo.Visible = false;
                    blockBillingPeriod.Visible = false;
                    blockSearch.Visible = false;
                    blockIncludeInactive.Visible = true;
                    break;

                case ReportTypes.RentCollection:
                    blockStatus.Visible = false;
                    blockProperty.Visible = false;
                    blockDateFrom.Visible = true;
                    blockDateTo.Visible = true;
                    blockBillingPeriod.Visible = false;
                    blockSearch.Visible = false;
                    blockIncludeInactive.Visible = false;
                    break;

                case ReportTypes.MonthlyDue:
                    blockStatus.Visible = true;
                    blockProperty.Visible = false;
                    blockDateFrom.Visible = false;
                    blockDateTo.Visible = false;
                    blockBillingPeriod.Visible = true;
                    blockSearch.Visible = false;
                    blockIncludeInactive.Visible = false;

                    cboStatusFilter.Items.AddRange(new object[] { "All", "Due", "Partial", "Paid", "Overdue", "Waived" });
                    cboStatusFilter.SelectedIndex = 0;
                    break;

                case ReportTypes.Agreement:
                    blockStatus.Visible = true;
                    blockProperty.Visible = false;
                    blockDateFrom.Visible = false;
                    blockDateTo.Visible = false;
                    blockBillingPeriod.Visible = false;
                    blockSearch.Visible = false;
                    blockIncludeInactive.Visible = false;

                    cboStatusFilter.Items.AddRange(new object[] { "All", "Draft", "Active", "Expired", "Terminated", "Cancelled" });
                    cboStatusFilter.SelectedIndex = 0;
                    break;

                case ReportTypes.IncomeSummary:
                    blockStatus.Visible = false;
                    blockProperty.Visible = false;
                    blockDateFrom.Visible = true;
                    blockDateTo.Visible = true;
                    blockBillingPeriod.Visible = false;
                    blockSearch.Visible = false;
                    blockIncludeInactive.Visible = false;
                    break;
            }

            pnlFiltersFlow.ResumeLayout();
            pnlFiltersFlow.PerformLayout();
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            if (cboReportType.SelectedValue == null)
            {
                ShowStatus("Please select a report type first.", false);
                return;
            }

            string reportType = cboReportType.SelectedValue.ToString();
            ReportDefinition def = _reports.Find(r => r.ReportType == reportType);
            
            if (def == null)
            {
                ShowStatus("Report definition not found.", false);
                return;
            }

            Cursor = Cursors.WaitCursor;
            ShowStatus("Generating report...", true);

            try
            {
                ServiceResult<DataTable> result = null;
                string filterSummary = "None";

                switch (reportType)
                {
                    case ReportTypes.TenantList:
                        var tenantFilter = new TenantListReportFilter
                        {
                            Status = cboStatusFilter.SelectedItem?.ToString(),
                            SearchText = txtSearch.Text
                        };
                        result = _reportService.GetTenantListReport(tenantFilter);
                        filterSummary = "Status: " + tenantFilter.Status + 
                                        (!string.IsNullOrWhiteSpace(tenantFilter.SearchText) ? ", Search: '" + tenantFilter.SearchText + "'" : "");
                        break;

                    case ReportTypes.PropertyOccupancy:
                        var occupancyFilter = new PropertyOccupancyReportFilter
                        {
                            PropertyId = cboPropertyFilter.SelectedValue as int?,
                            IncludeInactive = chkIncludeInactive.Checked
                        };
                        result = _reportService.GetPropertyOccupancyReport(occupancyFilter);
                        filterSummary = "Property: " + cboPropertyFilter.Text + 
                                        (occupancyFilter.IncludeInactive ? ", Inactive rooms included" : ", Active rooms only");
                        break;

                    case ReportTypes.RentCollection:
                        var collectionFilter = new RentCollectionReportFilter
                        {
                            DateFrom = dtpDateFrom.Value,
                            DateTo = dtpDateTo.Value
                        };
                        result = _reportService.GetRentCollectionReport(collectionFilter);
                        filterSummary = "Period: " + collectionFilter.DateFrom.ToString("yyyy-MM-dd") + " to " + collectionFilter.DateTo.ToString("yyyy-MM-dd");
                        break;

                    case ReportTypes.MonthlyDue:
                        var dueFilter = new MonthlyDueReportFilter
                        {
                            BillingPeriod = dtpBillingPeriod.Value,
                            ChargeStatus = cboStatusFilter.SelectedItem?.ToString()
                        };
                        result = _reportService.GetMonthlyDueReport(dueFilter);
                        filterSummary = "Billing month: " + dueFilter.BillingPeriod.ToString("yyyy-MM") + ", Status: " + dueFilter.ChargeStatus;
                        break;

                    case ReportTypes.Agreement:
                        var agreementFilter = new AgreementReportFilter
                        {
                            Status = cboStatusFilter.SelectedItem?.ToString()
                        };
                        result = _reportService.GetAgreementReport(agreementFilter);
                        filterSummary = "Status: " + agreementFilter.Status;
                        break;

                    case ReportTypes.IncomeSummary:
                        var incomeFilter = new IncomeSummaryReportFilter
                        {
                            DateFrom = dtpDateFrom.Value,
                            DateTo = dtpDateTo.Value
                        };
                        result = _reportService.GetIncomeSummaryReport(incomeFilter);
                        filterSummary = "Period: " + incomeFilter.DateFrom.ToString("yyyy-MM") + " to " + incomeFilter.DateTo.ToString("yyyy-MM");
                        break;
                }

                if (result == null || !result.IsSuccess)
                {
                    ShowStatus(result?.Message ?? "An error occurred during report generation.", false);
                    return;
                }

                RenderReport(def, result.Data, filterSummary);
                ShowStatus($"Report generated successfully. {result.Data.Rows.Count} record(s) loaded.", true);
            }
            catch (Exception ex)
            {
                ShowStatus("Unable to render report: " + ex.Message, false);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void RenderReport(ReportDefinition def, DataTable data, string filterSummary)
        {
            reportViewer.Reset();
            reportViewer.ProcessingMode = ProcessingMode.Local;
            
            // Set embedded report path
            reportViewer.LocalReport.ReportEmbeddedResource = def.RdlcFileName;

            // Bind data source
            ReportDataSource rds = new ReportDataSource(def.DataSetName, data);
            reportViewer.LocalReport.DataSources.Clear();
            reportViewer.LocalReport.DataSources.Add(rds);

            // Pass standard report parameters
            List<ReportParameter> parameters = new List<ReportParameter>
            {
                new ReportParameter("ReportTitle", def.DisplayName),
                new ReportParameter("FilterSummary", filterSummary),
                new ReportParameter("GeneratedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                new ReportParameter("GeneratedBy", CurrentSession.User?.FullName ?? "System"),
                new ReportParameter("Currency", _reportService.GetDefaultCurrency()),
                new ReportParameter("ApplicationName", "House Rental Management System")
            };

            reportViewer.LocalReport.SetParameters(parameters);
            
            // Refresh and display
            reportViewer.RefreshReport();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ResetFilters();
            ShowStatus("Filters cleared.", true);
        }

        private void ShowStatus(string message, bool isSuccess)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = isSuccess ? Color.FromArgb(71, 85, 105) : Color.Firebrick;
        }
    }
}
