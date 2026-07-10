using System;
using System.Data;
using Housing_rental.Models;

namespace Housing_rental.DAL
{
    public class DashboardRepository
    {
        public DashboardSummary GetSummary()
        {
            const string sql = @"
EXEC dbo.sp_GetDashboardSummary;";

            DataTable table = SqlHelper.ExecuteDataTable(sql);

            if (table.Rows.Count == 0)
            {
                return new DashboardSummary();
            }

            DataRow row = table.Rows[0];

            return new DashboardSummary
            {
                TotalProperties = Convert.ToInt32(row["TotalProperties"]),
                TotalHouses = Convert.ToInt32(row["TotalHouses"]),
                TotalRooms = Convert.ToInt32(row["TotalRooms"]),
                AvailableRooms = Convert.ToInt32(row["AvailableRooms"]),
                OccupiedRooms = Convert.ToInt32(row["OccupiedRooms"]),
                TotalTenants = Convert.ToInt32(row["TotalTenants"]),
                ActiveAgreements = Convert.ToInt32(row["ActiveAgreements"]),
                MonthlyExpectedRent = Convert.ToDecimal(row["MonthlyExpectedRent"]),
                MonthlyCollectedRent = Convert.ToDecimal(row["MonthlyCollectedRent"]),
                MonthlyDueRent = Convert.ToDecimal(row["MonthlyDueRent"]),
                OverduePayments = Convert.ToInt32(row["OverduePayments"])
            };
        }
    }
}
