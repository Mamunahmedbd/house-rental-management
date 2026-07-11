using System;

namespace Housing_rental.Models
{
    // Report type enumeration
    public static class ReportTypes
    {
        public const string TenantList = "TenantList";
        public const string PropertyOccupancy = "PropertyOccupancy";
        public const string RentCollection = "RentCollection";
        public const string MonthlyDue = "MonthlyDue";
        public const string Agreement = "Agreement";
        public const string IncomeSummary = "IncomeSummary";
    }

    // Filter DTOs
    public class TenantListReportFilter
    {
        public string Status { get; set; }
        public string SearchText { get; set; }
    }

    public class PropertyOccupancyReportFilter
    {
        public int? PropertyId { get; set; }
        public bool IncludeInactive { get; set; }
    }

    public class RentCollectionReportFilter
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }

    public class MonthlyDueReportFilter
    {
        public DateTime BillingPeriod { get; set; }
        public string ChargeStatus { get; set; }
    }

    public class AgreementReportFilter
    {
        public string Status { get; set; }
    }

    public class IncomeSummaryReportFilter
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }

    // Report definition metadata
    public class ReportDefinition
    {
        public string ReportType { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string RdlcFileName { get; set; }
        public string DataSetName { get; set; }
    }
}
