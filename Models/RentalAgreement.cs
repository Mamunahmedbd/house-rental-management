using System;

namespace Housing_rental.Models
{
    public class RentalAgreement
    {
        public int AgreementId { get; set; }
        public string AgreementNo { get; set; }
        public int TenantId { get; set; }
        public int RoomId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public decimal SecurityDeposit { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
