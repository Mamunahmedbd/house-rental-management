using System;
using System.Collections.Generic;

namespace Housing_rental.Models
{
    public class PaymentAgreementItem
    {
        public int AgreementId { get; set; }
        public string AgreementNo { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public string TenantPhone { get; set; }
        public string PropertyName { get; set; }
        public string HouseName { get; set; }
        public string RoomNo { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public string AgreementStatus { get; set; }
        public decimal TotalDue { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal OverdueAmount { get; set; }

        public string RoomPath
        {
            get { return PropertyName + " / " + HouseName + " / " + RoomNo; }
        }
    }

    public class RentChargeListItem
    {
        public long ChargeId { get; set; }
        public int AgreementId { get; set; }
        public string AgreementNo { get; set; }
        public string TenantName { get; set; }
        public string PropertyName { get; set; }
        public string HouseName { get; set; }
        public string RoomNo { get; set; }
        public string ChargeType { get; set; }
        public DateTime BillingPeriod { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public string CurrencyCode { get; set; }
        public string ChargeStatus { get; set; }
        public bool Selected { get; set; }
        public decimal AllocationAmount { get; set; }
    }

    public class PaymentListItem
    {
        public long PaymentId { get; set; }
        public string ReceiptNo { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public int AgreementId { get; set; }
        public string AgreementNo { get; set; }
        public string PropertyName { get; set; }
        public string HouseName { get; set; }
        public string RoomNo { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime PostedAt { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string PaymentMethod { get; set; }
        public string ExternalReference { get; set; }
        public string Status { get; set; }
        public string CollectedByName { get; set; }
        public string ReversalReason { get; set; }
        public DateTime? ReversedAt { get; set; }
        public string ReversedByName { get; set; }
    }

    public class PaymentAllocationDetail
    {
        public long ChargeId { get; set; }
        public DateTime BillingPeriod { get; set; }
        public DateTime DueDate { get; set; }
        public decimal ChargeAmount { get; set; }
        public decimal AllocatedAmount { get; set; }
        public string CurrencyCode { get; set; }
    }

    public class PaymentAllocationRequest
    {
        public long ChargeId { get; set; }
        public decimal Amount { get; set; }
    }

    public class PostPaymentRequest
    {
        public Guid RequestId { get; set; }
        public int TenantId { get; set; }
        public int AgreementId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string PaymentMethod { get; set; }
        public string ExternalReference { get; set; }
        public string Remarks { get; set; }
        public List<PaymentAllocationRequest> Allocations { get; set; }

        public PostPaymentRequest()
        {
            RequestId = Guid.NewGuid();
            Allocations = new List<PaymentAllocationRequest>();
        }
    }

    public class PostPaymentResult
    {
        public long PaymentId { get; set; }
        public string ReceiptNo { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string Status { get; set; }
        public bool AlreadyProcessed { get; set; }
    }

    public class ReversePaymentRequest
    {
        public long PaymentId { get; set; }
        public Guid RequestId { get; set; }
        public string Reason { get; set; }

        public ReversePaymentRequest()
        {
            RequestId = Guid.NewGuid();
        }
    }

    public class ChargeGenerationResult
    {
        public int CreatedCount { get; set; }
        public DateTime BillingPeriod { get; set; }
        public Guid GenerationRunId { get; set; }
    }
}
