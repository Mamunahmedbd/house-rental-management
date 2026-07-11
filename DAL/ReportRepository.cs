using System;
using System.Data;
using System.Data.SqlClient;

namespace Housing_rental.DAL
{
    public class ReportRepository
    {
        public DataTable GetTenantListData(string status, string searchText)
        {
            const string sql = "EXEC dbo.sp_Rpt_TenantList @Status, @SearchText;";
            return SqlHelper.ExecuteDataTable(sql,
                SqlHelper.Parameter("@Status", status),
                SqlHelper.Parameter("@SearchText", searchText));
        }

        public DataTable GetPropertyOccupancyData(int? propertyId, bool includeInactive)
        {
            const string sql = "EXEC dbo.sp_Rpt_PropertyOccupancy @PropertyId, @IncludeInactive;";
            return SqlHelper.ExecuteDataTable(sql,
                SqlHelper.Parameter("@PropertyId", propertyId),
                SqlHelper.Parameter("@IncludeInactive", includeInactive));
        }

        public DataTable GetRentCollectionData(DateTime dateFrom, DateTime dateTo)
        {
            const string sql = "EXEC dbo.sp_GetRentCollectionReport @DateFrom, @DateTo;";
            return SqlHelper.ExecuteDataTable(sql,
                SqlHelper.Parameter("@DateFrom", dateFrom.Date),
                SqlHelper.Parameter("@DateTo", dateTo.Date));
        }

        public DataTable GetMonthlyDueData(DateTime billingPeriod, string chargeStatus)
        {
            const string sql = "EXEC dbo.sp_Rpt_MonthlyDue @BillingPeriod, @ChargeStatus;";
            return SqlHelper.ExecuteDataTable(sql,
                SqlHelper.Parameter("@BillingPeriod", new DateTime(billingPeriod.Year, billingPeriod.Month, 1)),
                SqlHelper.Parameter("@ChargeStatus", chargeStatus));
        }

        public DataTable GetAgreementData(string status)
        {
            const string sql = @"
SELECT
    AgreementId,
    AgreementNo,
    TenantId,
    TenantName,
    TenantPhone,
    TenantStatus,
    PropertyId,
    PropertyName,
    HouseId,
    HouseName,
    RoomId,
    RoomNo,
    RoomType,
    RoomStatus,
    StartDate,
    EndDate,
    MonthlyRent,
    SecurityDeposit,
    AgreementStatus,
    CreatedByUserId,
    CreatedByName,
    CreatedAt,
    TotalDue,
    TotalPaid,
    TotalBalance,
    OverdueAmount,
    OverdueCount,
    PaymentCount
FROM dbo.vw_AgreementDirectory
WHERE (@Status IS NULL OR @Status = '' OR @Status = 'All' OR AgreementStatus = @Status)
ORDER BY StartDate DESC, AgreementNo;";

            return SqlHelper.ExecuteDataTable(sql,
                SqlHelper.Parameter("@Status", status));
        }

        public DataTable GetIncomeSummaryData(DateTime dateFrom, DateTime dateTo)
        {
            const string sql = "EXEC dbo.sp_Rpt_IncomeSummary @DateFrom, @DateTo;";
            return SqlHelper.ExecuteDataTable(sql,
                SqlHelper.Parameter("@DateFrom", new DateTime(dateFrom.Year, dateFrom.Month, 1)),
                SqlHelper.Parameter("@DateTo", new DateTime(dateTo.Year, dateTo.Month, 1)));
        }

        public DataTable GetActiveProperties()
        {
            const string sql = @"
SELECT PropertyId, PropertyName
FROM dbo.Properties
WHERE IsActive = 1
ORDER BY PropertyName;";

            return SqlHelper.ExecuteDataTable(sql);
        }
    }
}
