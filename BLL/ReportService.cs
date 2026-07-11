using System;
using System.Collections.Generic;
using System.Data;
using Housing_rental.DAL;
using Housing_rental.Models;

namespace Housing_rental.BLL
{
    public class ReportService
    {
        private readonly ReportRepository _reportRepository;
        private readonly AuditRepository _auditRepository;

        public ReportService()
        {
            _reportRepository = new ReportRepository();
            _auditRepository = new AuditRepository();
        }

        public List<ReportDefinition> GetReportDefinitions()
        {
            return new List<ReportDefinition>
            {
                new ReportDefinition
                {
                    ReportType = ReportTypes.TenantList,
                    DisplayName = "Tenant List Report",
                    Description = "Master list of tenants with contact details and receivable summary",
                    RdlcFileName = "Housing_rental.Reports.TenantListReport.rdlc",
                    DataSetName = "TenantDataSet"
                },
                new ReportDefinition
                {
                    ReportType = ReportTypes.PropertyOccupancy,
                    DisplayName = "Property Occupancy Report",
                    Description = "Property/house/room occupancy status with tenant assignments",
                    RdlcFileName = "Housing_rental.Reports.PropertyOccupancyReport.rdlc",
                    DataSetName = "OccupancyDataSet"
                },
                new ReportDefinition
                {
                    ReportType = ReportTypes.RentCollection,
                    DisplayName = "Rent Collection Report",
                    Description = "Payment receipts within a date range with collection method and collector",
                    RdlcFileName = "Housing_rental.Reports.RentCollectionReport.rdlc",
                    DataSetName = "CollectionDataSet"
                },
                new ReportDefinition
                {
                    ReportType = ReportTypes.MonthlyDue,
                    DisplayName = "Monthly Due Report",
                    Description = "Outstanding charges for a specific billing period with aging status",
                    RdlcFileName = "Housing_rental.Reports.MonthlyDueReport.rdlc",
                    DataSetName = "MonthlyDueDataSet"
                },
                new ReportDefinition
                {
                    ReportType = ReportTypes.Agreement,
                    DisplayName = "Agreement Report",
                    Description = "Active and historical agreements with financial position summary",
                    RdlcFileName = "Housing_rental.Reports.AgreementReport.rdlc",
                    DataSetName = "AgreementDataSet"
                },
                new ReportDefinition
                {
                    ReportType = ReportTypes.IncomeSummary,
                    DisplayName = "Income Summary Report",
                    Description = "Revenue, expenses, and net income aggregation by month",
                    RdlcFileName = "Housing_rental.Reports.IncomeSummaryReport.rdlc",
                    DataSetName = "IncomeDataSet"
                }
            };
        }

        public ServiceResult<DataTable> GetTenantListReport(TenantListReportFilter filter)
        {
            if (!CurrentSession.IsAuthenticated)
                return ServiceResult<DataTable>.Failure("Please sign in before viewing reports.");

            if (filter == null)
                filter = new TenantListReportFilter();

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                filter.Status != "All" &&
                filter.Status != "Active" &&
                filter.Status != "Inactive" &&
                filter.Status != "Blacklisted")
            {
                return ServiceResult<DataTable>.Failure("Please select a valid tenant status.");
            }

            try
            {
                DataTable data = _reportRepository.GetTenantListData(Clean(filter.Status), Clean(filter.SearchText));
                TryAudit("Generate Report", "Reports", "TenantList", 
                    "Generated Tenant List. Status: " + (filter.Status ?? "All") + ", Search: '" + (filter.SearchText ?? "") + "'. Records: " + data.Rows.Count);
                return ServiceResult<DataTable>.Success(data, "Tenant List Report generated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<DataTable>.Failure("Unable to generate the tenant list report. " + ex.Message);
            }
        }

        public ServiceResult<DataTable> GetPropertyOccupancyReport(PropertyOccupancyReportFilter filter)
        {
            if (!CurrentSession.IsAuthenticated)
                return ServiceResult<DataTable>.Failure("Please sign in before viewing reports.");

            if (filter == null)
                filter = new PropertyOccupancyReportFilter();

            if (filter.PropertyId.HasValue && filter.PropertyId.Value <= 0)
                return ServiceResult<DataTable>.Failure("Please select a valid property.");

            try
            {
                DataTable data = _reportRepository.GetPropertyOccupancyData(filter.PropertyId, filter.IncludeInactive);
                TryAudit("Generate Report", "Reports", "PropertyOccupancy", 
                    "Generated Property Occupancy. PropertyId: " + (filter.PropertyId?.ToString() ?? "All") + ". Records: " + data.Rows.Count);
                return ServiceResult<DataTable>.Success(data, "Property Occupancy Report generated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<DataTable>.Failure("Unable to generate the property occupancy report. " + ex.Message);
            }
        }

        public ServiceResult<DataTable> GetRentCollectionReport(RentCollectionReportFilter filter)
        {
            if (!CurrentSession.IsAuthenticated)
                return ServiceResult<DataTable>.Failure("Please sign in before viewing reports.");

            if (filter == null)
                return ServiceResult<DataTable>.Failure("Invalid report filters.");

            if (filter.DateFrom.Date > filter.DateTo.Date)
                return ServiceResult<DataTable>.Failure("The start date cannot be after the end date.");

            try
            {
                DataTable data = _reportRepository.GetRentCollectionData(filter.DateFrom, filter.DateTo);
                TryAudit("Generate Report", "Reports", "RentCollection", 
                    "Generated Rent Collection. From: " + filter.DateFrom.ToString("yyyy-MM-dd") + ", To: " + filter.DateTo.ToString("yyyy-MM-dd") + ". Records: " + data.Rows.Count);
                return ServiceResult<DataTable>.Success(data, "Rent Collection Report generated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<DataTable>.Failure("Unable to generate the rent collection report. " + ex.Message);
            }
        }

        public ServiceResult<DataTable> GetMonthlyDueReport(MonthlyDueReportFilter filter)
        {
            if (!CurrentSession.IsAuthenticated)
                return ServiceResult<DataTable>.Failure("Please sign in before viewing reports.");

            if (filter == null)
                return ServiceResult<DataTable>.Failure("Invalid report filters.");

            if (!string.IsNullOrWhiteSpace(filter.ChargeStatus) &&
                filter.ChargeStatus != "All" &&
                filter.ChargeStatus != "Due" &&
                filter.ChargeStatus != "Partial" &&
                filter.ChargeStatus != "Paid" &&
                filter.ChargeStatus != "Overdue" &&
                filter.ChargeStatus != "Waived")
            {
                return ServiceResult<DataTable>.Failure("Please select a valid charge status.");
            }

            try
            {
                DataTable data = _reportRepository.GetMonthlyDueData(filter.BillingPeriod, Clean(filter.ChargeStatus));
                TryAudit("Generate Report", "Reports", "MonthlyDue", 
                    "Generated Monthly Due. Period: " + filter.BillingPeriod.ToString("yyyy-MM") + ", Status: " + (filter.ChargeStatus ?? "All") + ". Records: " + data.Rows.Count);
                return ServiceResult<DataTable>.Success(data, "Monthly Due Report generated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<DataTable>.Failure("Unable to generate the monthly due report. " + ex.Message);
            }
        }

        public ServiceResult<DataTable> GetAgreementReport(AgreementReportFilter filter)
        {
            if (!CurrentSession.IsAuthenticated)
                return ServiceResult<DataTable>.Failure("Please sign in before viewing reports.");

            if (filter == null)
                filter = new AgreementReportFilter();

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                filter.Status != "All" &&
                filter.Status != "Draft" &&
                filter.Status != "Active" &&
                filter.Status != "Expired" &&
                filter.Status != "Terminated" &&
                filter.Status != "Cancelled")
            {
                return ServiceResult<DataTable>.Failure("Please select a valid agreement status.");
            }

            try
            {
                DataTable data = _reportRepository.GetAgreementData(Clean(filter.Status));
                TryAudit("Generate Report", "Reports", "AgreementList", 
                    "Generated Agreement list. Status: " + (filter.Status ?? "All") + ". Records: " + data.Rows.Count);
                return ServiceResult<DataTable>.Success(data, "Agreement Report generated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<DataTable>.Failure("Unable to generate the agreement report. " + ex.Message);
            }
        }

        public ServiceResult<DataTable> GetIncomeSummaryReport(IncomeSummaryReportFilter filter)
        {
            if (!CurrentSession.IsAuthenticated)
                return ServiceResult<DataTable>.Failure("Please sign in before viewing reports.");

            if (filter == null)
                return ServiceResult<DataTable>.Failure("Invalid report filters.");

            if (filter.DateFrom.Date > filter.DateTo.Date)
                return ServiceResult<DataTable>.Failure("The start date cannot be after the end date.");

            int monthsDifference = ((filter.DateTo.Year - filter.DateFrom.Year) * 12) + filter.DateTo.Month - filter.DateFrom.Month;
            if (monthsDifference > 24)
                return ServiceResult<DataTable>.Failure("The income summary report range cannot exceed 24 months.");

            try
            {
                DataTable data = _reportRepository.GetIncomeSummaryData(filter.DateFrom, filter.DateTo);
                TryAudit("Generate Report", "Reports", "IncomeSummary", 
                    "Generated Income Summary. From: " + filter.DateFrom.ToString("yyyy-MM") + ", To: " + filter.DateTo.ToString("yyyy-MM") + ". Records: " + data.Rows.Count);
                return ServiceResult<DataTable>.Success(data, "Income Summary Report generated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<DataTable>.Failure("Unable to generate the income summary report. " + ex.Message);
            }
        }

        public ServiceResult<DataTable> GetActiveProperties()
        {
            try
            {
                DataTable data = _reportRepository.GetActiveProperties();
                return ServiceResult<DataTable>.Success(data, "Properties loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<DataTable>.Failure("Unable to load properties. " + ex.Message);
            }
        }

        public string GetDefaultCurrency()
        {
            try
            {
                return new RentPaymentService().GetDefaultCurrency();
            }
            catch
            {
                return "USD";
            }
        }

        private void TryAudit(string actionName, string tableName, string recordId, string description)
        {
            try
            {
                _auditRepository.Add(CurrentSession.User?.UserId, actionName, tableName, recordId, description);
            }
            catch
            {
                // Best effort
            }
        }

        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
