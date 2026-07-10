using System;

namespace Housing_rental.Models
{
    public class RentPayment
    {
        public int PaymentId { get; set; }
        public string ReceiptNo { get; set; }
        public int AgreementId { get; set; }
        public int PaymentMonth { get; set; }
        public int PaymentYear { get; set; }
        public decimal DueAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public int CollectedByUserId { get; set; }
        public string Remarks { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
