namespace Housing_rental.Models
{
    public class DashboardSummary
    {
        public int TotalProperties { get; set; }
        public int TotalHouses { get; set; }
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int TotalTenants { get; set; }
        public int ActiveAgreements { get; set; }
        public decimal MonthlyExpectedRent { get; set; }
        public decimal MonthlyCollectedRent { get; set; }
        public decimal MonthlyDueRent { get; set; }
        public int OverduePayments { get; set; }
    }
}
