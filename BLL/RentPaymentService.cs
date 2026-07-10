using Housing_rental.Models;

namespace Housing_rental.BLL
{
    public class RentPaymentService
    {
        public ServiceResult ValidatePayment(RentPayment payment)
        {
            if (payment == null)
            {
                return ServiceResult.Failure("Payment information is required.");
            }

            if (payment.AgreementId <= 0)
            {
                return ServiceResult.Failure("A valid agreement is required.");
            }

            if (payment.PaymentMonth < 1 || payment.PaymentMonth > 12)
            {
                return ServiceResult.Failure("Payment month must be between 1 and 12.");
            }

            if (payment.PaymentYear < 2000)
            {
                return ServiceResult.Failure("Payment year is invalid.");
            }

            if (payment.DueAmount <= 0)
            {
                return ServiceResult.Failure("Due amount must be greater than zero.");
            }

            if (payment.PaidAmount < 0)
            {
                return ServiceResult.Failure("Paid amount cannot be negative.");
            }

            if (payment.PaidAmount > payment.DueAmount)
            {
                return ServiceResult.Failure("Paid amount cannot be greater than due amount.");
            }

            payment.BalanceAmount = payment.DueAmount - payment.PaidAmount;
            payment.Status = payment.BalanceAmount == 0 ? "Paid" : payment.PaidAmount == 0 ? "Pending" : "Partial";

            return ServiceResult.Success("Payment information is valid.");
        }
    }
}
