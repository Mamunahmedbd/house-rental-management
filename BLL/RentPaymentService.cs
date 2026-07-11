using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Housing_rental.DAL;
using Housing_rental.Models;

namespace Housing_rental.BLL
{
    public class RentPaymentService
    {
        private readonly RentPaymentRepository _paymentRepository;
        private readonly AuditRepository _auditRepository;

        public RentPaymentService()
        {
            _paymentRepository = new RentPaymentRepository();
            _auditRepository = new AuditRepository();
        }

        public ServiceResult<List<PaymentAgreementItem>> SearchAgreementContexts(string searchText)
        {
            ServiceResult authorization = AuthorizeView();
            if (!authorization.IsSuccess)
            {
                return ServiceResult<List<PaymentAgreementItem>>.Failure(authorization.Message);
            }

            try
            {
                List<PaymentAgreementItem> items = _paymentRepository.SearchAgreementContexts(Clean(searchText));
                return ServiceResult<List<PaymentAgreementItem>>.Success(items, "Payment agreements loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<List<PaymentAgreementItem>>.Failure(ToSafeMessage(ex, "Unable to load payment agreements."));
            }
        }

        public ServiceResult<List<RentChargeListItem>> SearchCharges(string searchText, string chargeStatus, int? agreementId, bool includePaid)
        {
            ServiceResult authorization = AuthorizeView();
            if (!authorization.IsSuccess)
            {
                return ServiceResult<List<RentChargeListItem>>.Failure(authorization.Message);
            }

            if (agreementId.HasValue && agreementId.Value <= 0)
            {
                return ServiceResult<List<RentChargeListItem>>.Failure("Please select a valid agreement.");
            }

            if (!IsValidChargeStatusFilter(chargeStatus))
            {
                return ServiceResult<List<RentChargeListItem>>.Failure("Please select a valid charge status.");
            }

            try
            {
                List<RentChargeListItem> items = _paymentRepository.SearchCharges(
                    Clean(searchText),
                    Clean(chargeStatus),
                    agreementId,
                    includePaid);

                return ServiceResult<List<RentChargeListItem>>.Success(items, "Rent charges loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<List<RentChargeListItem>>.Failure(ToSafeMessage(ex, "Unable to load rent charges."));
            }
        }

        public ServiceResult<List<PaymentListItem>> SearchPayments(string searchText, string status, DateTime? dateFrom, DateTime? dateTo)
        {
            ServiceResult authorization = AuthorizeView();
            if (!authorization.IsSuccess)
            {
                return ServiceResult<List<PaymentListItem>>.Failure(authorization.Message);
            }

            if (dateFrom.HasValue && dateTo.HasValue && dateFrom.Value.Date > dateTo.Value.Date)
            {
                return ServiceResult<List<PaymentListItem>>.Failure("The history start date cannot be after the end date.");
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All" && status != PaymentStatuses.Posted && status != PaymentStatuses.Reversed)
            {
                return ServiceResult<List<PaymentListItem>>.Failure("Please select a valid payment status.");
            }

            try
            {
                List<PaymentListItem> items = _paymentRepository.SearchPayments(
                    Clean(searchText),
                    Clean(status),
                    dateFrom.HasValue ? dateFrom.Value.Date : (DateTime?)null,
                    dateTo.HasValue ? dateTo.Value.Date : (DateTime?)null);

                return ServiceResult<List<PaymentListItem>>.Success(items, "Payment history loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<List<PaymentListItem>>.Failure(ToSafeMessage(ex, "Unable to load payment history."));
            }
        }

        public ServiceResult<List<PaymentAllocationDetail>> GetPaymentAllocations(long paymentId)
        {
            ServiceResult authorization = AuthorizeView();
            if (!authorization.IsSuccess)
            {
                return ServiceResult<List<PaymentAllocationDetail>>.Failure(authorization.Message);
            }

            if (paymentId <= 0)
            {
                return ServiceResult<List<PaymentAllocationDetail>>.Failure("Please select a valid payment.");
            }

            try
            {
                List<PaymentAllocationDetail> items = _paymentRepository.GetPaymentAllocations(paymentId);
                return ServiceResult<List<PaymentAllocationDetail>>.Success(items, "Payment allocations loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<List<PaymentAllocationDetail>>.Failure(ToSafeMessage(ex, "Unable to load payment details."));
            }
        }

        public ServiceResult<PaymentListItem> GetPaymentById(long paymentId)
        {
            ServiceResult authorization = AuthorizeView();
            if (!authorization.IsSuccess)
            {
                return ServiceResult<PaymentListItem>.Failure(authorization.Message);
            }

            if (paymentId <= 0)
            {
                return ServiceResult<PaymentListItem>.Failure("Please select a valid payment.");
            }

            try
            {
                PaymentListItem payment = _paymentRepository.GetPaymentById(paymentId);
                return payment == null
                    ? ServiceResult<PaymentListItem>.Failure("The selected payment was not found.")
                    : ServiceResult<PaymentListItem>.Success(payment, "Payment loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<PaymentListItem>.Failure(ToSafeMessage(ex, "Unable to load the payment."));
            }
        }

        public ServiceResult<PostPaymentResult> PostPayment(PostPaymentRequest request)
        {
            ServiceResult authorization = AuthorizeCollection();
            if (!authorization.IsSuccess)
            {
                return ServiceResult<PostPaymentResult>.Failure(authorization.Message);
            }

            ServiceResult validation = ValidatePostRequest(request);
            if (!validation.IsSuccess)
            {
                return ServiceResult<PostPaymentResult>.Failure(validation.Message);
            }

            try
            {
                NormalizeRequest(request);
                PostPaymentResult result = _paymentRepository.PostPayment(request, CurrentSession.User.UserId);

                TryAudit(
                    "Post Payment",
                    "Payments",
                    result.PaymentId.ToString(),
                    "Posted receipt '" + result.ReceiptNo + "' for " + result.Amount.ToString("0.00") + " " + result.CurrencyCode + ".");

                string message = result.AlreadyProcessed
                    ? "This request was already completed as receipt " + result.ReceiptNo + "."
                    : "Payment posted successfully as receipt " + result.ReceiptNo + ".";

                return ServiceResult<PostPaymentResult>.Success(result, message);
            }
            catch (Exception ex)
            {
                return ServiceResult<PostPaymentResult>.Failure(ToSafeMessage(ex, "Unable to confirm the payment. No new receipt has been confirmed."));
            }
        }

        public ServiceResult<PostPaymentResult> ReversePayment(ReversePaymentRequest request)
        {
            if (!CurrentSession.IsAuthenticated)
            {
                return ServiceResult<PostPaymentResult>.Failure("Please sign in before reversing a payment.");
            }

            if (!CurrentSession.IsAdmin)
            {
                return ServiceResult<PostPaymentResult>.Failure("Only an administrator can reverse a posted payment.");
            }

            if (request == null || request.PaymentId <= 0)
            {
                return ServiceResult<PostPaymentResult>.Failure("Please select a valid payment.");
            }

            if (request.RequestId == Guid.Empty)
            {
                request.RequestId = Guid.NewGuid();
            }

            request.Reason = Clean(request.Reason);
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return ServiceResult<PostPaymentResult>.Failure("A reversal reason is required.");
            }

            if (request.Reason.Length > 500)
            {
                return ServiceResult<PostPaymentResult>.Failure("The reversal reason cannot exceed 500 characters.");
            }

            try
            {
                PostPaymentResult result = _paymentRepository.ReversePayment(request, CurrentSession.User.UserId);
                TryAudit("Reverse Payment", "Payments", result.PaymentId.ToString(), "Reversed receipt '" + result.ReceiptNo + "'. Reason: " + request.Reason);

                string message = result.AlreadyProcessed
                    ? "This reversal request was already completed for receipt " + result.ReceiptNo + "."
                    : "Receipt " + result.ReceiptNo + " was reversed successfully.";

                return ServiceResult<PostPaymentResult>.Success(result, message);
            }
            catch (Exception ex)
            {
                return ServiceResult<PostPaymentResult>.Failure(ToSafeMessage(ex, "Unable to reverse the payment."));
            }
        }

        public ServiceResult<ChargeGenerationResult> GenerateMonthlyCharges(DateTime billingPeriod)
        {
            if (!CurrentSession.IsAuthenticated)
            {
                return ServiceResult<ChargeGenerationResult>.Failure("Please sign in before generating charges.");
            }

            if (!CurrentSession.IsAdmin)
            {
                return ServiceResult<ChargeGenerationResult>.Failure("Only an administrator can generate monthly rent charges.");
            }

            DateTime normalizedPeriod = new DateTime(billingPeriod.Year, billingPeriod.Month, 1);
            if (normalizedPeriod > new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(12))
            {
                return ServiceResult<ChargeGenerationResult>.Failure("Charges cannot be generated more than twelve months in advance.");
            }

            try
            {
                Guid runId = Guid.NewGuid();
                ChargeGenerationResult result = _paymentRepository.GenerateMonthlyCharges(normalizedPeriod, CurrentSession.User.UserId, runId);
                TryAudit("Generate Monthly Rent Charges", "RentCharges", null, "Generated " + result.CreatedCount + " rent charge(s) for " + result.BillingPeriod.ToString("yyyy-MM") + ". Run: " + runId + ".");

                string message = result.CreatedCount == 0
                    ? "No missing charges were found for " + result.BillingPeriod.ToString("MMMM yyyy") + "."
                    : result.CreatedCount + " rent charge(s) generated for " + result.BillingPeriod.ToString("MMMM yyyy") + ".";

                return ServiceResult<ChargeGenerationResult>.Success(result, message);
            }
            catch (Exception ex)
            {
                return ServiceResult<ChargeGenerationResult>.Failure(ToSafeMessage(ex, "Unable to generate monthly rent charges."));
            }
        }

        public string GetDefaultCurrency()
        {
            try
            {
                string value = _paymentRepository.GetSetting("DefaultCurrency", "USD");
                value = string.IsNullOrWhiteSpace(value) ? "USD" : value.Trim().ToUpperInvariant();
                return value.Length == 3 ? value : "USD";
            }
            catch
            {
                return "USD";
            }
        }

        public string GetReceiptFooter()
        {
            try
            {
                return _paymentRepository.GetSetting("ReceiptFooter", "Thank you for your payment.");
            }
            catch
            {
                return "Thank you for your payment.";
            }
        }

        private static ServiceResult ValidatePostRequest(PostPaymentRequest request)
        {
            if (request == null)
            {
                return ServiceResult.Failure("Payment information is required.");
            }

            if (request.RequestId == Guid.Empty)
            {
                request.RequestId = Guid.NewGuid();
            }

            if (request.TenantId <= 0 || request.AgreementId <= 0)
            {
                return ServiceResult.Failure("A valid tenant and agreement are required.");
            }

            if (request.PaymentDate.Date > DateTime.Today)
            {
                return ServiceResult.Failure("Payment date cannot be in the future.");
            }

            if (!CurrentSession.IsAdmin && request.PaymentDate.Date != DateTime.Today)
            {
                return ServiceResult.Failure("Only an administrator can post a backdated payment.");
            }

            if (request.Amount <= 0)
            {
                return ServiceResult.Failure("Payment amount must be greater than zero.");
            }

            if (decimal.Round(request.Amount, 2) != request.Amount)
            {
                return ServiceResult.Failure("Payment amount cannot contain more than two decimal places.");
            }

            if (string.IsNullOrWhiteSpace(request.CurrencyCode) || request.CurrencyCode.Trim().Length != 3)
            {
                return ServiceResult.Failure("A valid three-letter currency code is required.");
            }

            if (!PaymentMethods.IsValid(Clean(request.PaymentMethod)))
            {
                return ServiceResult.Failure("Please select a valid payment method.");
            }

            if (request.PaymentMethod != PaymentMethods.Cash && string.IsNullOrWhiteSpace(request.ExternalReference))
            {
                return ServiceResult.Failure("An external reference is required for non-cash payments.");
            }

            if (!string.IsNullOrWhiteSpace(request.ExternalReference) && request.ExternalReference.Trim().Length > 100)
            {
                return ServiceResult.Failure("The external reference cannot exceed 100 characters.");
            }

            if (!string.IsNullOrWhiteSpace(request.Remarks) && request.Remarks.Trim().Length > 300)
            {
                return ServiceResult.Failure("Remarks cannot exceed 300 characters.");
            }

            if (request.Allocations == null || request.Allocations.Count == 0)
            {
                return ServiceResult.Failure("Select at least one outstanding rent charge.");
            }

            if (request.Allocations.Any(item => item.ChargeId <= 0 || item.Amount <= 0 || decimal.Round(item.Amount, 2) != item.Amount))
            {
                return ServiceResult.Failure("Every allocation must have a valid charge and positive two-decimal amount.");
            }

            if (request.Allocations.GroupBy(item => item.ChargeId).Any(group => group.Count() > 1))
            {
                return ServiceResult.Failure("A charge can appear only once in a payment.");
            }

            if (request.Allocations.Sum(item => item.Amount) != request.Amount)
            {
                return ServiceResult.Failure("Allocation total must equal the payment amount.");
            }

            return ServiceResult.Success("Payment request is valid.");
        }

        private static void NormalizeRequest(PostPaymentRequest request)
        {
            request.PaymentDate = request.PaymentDate.Date;
            request.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
            request.PaymentMethod = request.PaymentMethod.Trim();
            request.ExternalReference = CleanOptional(request.ExternalReference);
            request.Remarks = CleanOptional(request.Remarks);
        }

        private static ServiceResult AuthorizeView()
        {
            return CurrentSession.IsAuthenticated
                ? ServiceResult.Success("Authorized.")
                : ServiceResult.Failure("Please sign in before viewing payments.");
        }

        private static ServiceResult AuthorizeCollection()
        {
            if (!CurrentSession.IsAuthenticated)
            {
                return ServiceResult.Failure("Please sign in before collecting a payment.");
            }

            string role = CurrentSession.User.RoleName;
            return role == "Admin" || role == "Staff"
                ? ServiceResult.Success("Authorized.")
                : ServiceResult.Failure("Your role is not authorized to collect payments.");
        }

        private static bool IsValidChargeStatusFilter(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                || value == ChargeStatuses.All
                || value == ChargeStatuses.Due
                || value == ChargeStatuses.Partial
                || value == ChargeStatuses.Paid
                || value == ChargeStatuses.Overdue
                || value == ChargeStatuses.Waived;
        }

        private static string ToSafeMessage(Exception exception, string fallback)
        {
            SqlException sqlException = exception as SqlException;
            if (sqlException == null)
            {
                return fallback;
            }

            switch (sqlException.Number)
            {
                case 51201: return "Payment amount must be greater than zero.";
                case 51202: return "The current collector is inactive. Sign in again.";
                case 51203: return "The selected tenant and agreement no longer match.";
                case 51204: return "The selected payment method is invalid.";
                case 51205: return "An external reference is required for non-cash payments.";
                case 51206: return "Select at least one charge to pay.";
                case 51207: return "Allocation amounts must be greater than zero.";
                case 51208: return "Allocation total must equal the payment amount.";
                case 51209: return "A selected charge no longer exists. Refresh and try again.";
                case 51210: return "A selected charge is no longer eligible for this payment.";
                case 51211: return "A selected balance changed because another payment was posted. Refresh and review the allocation.";
                case 51301: return "A reversal reason is required.";
                case 51302: return "Only an active administrator can reverse a payment.";
                case 51303: return "The selected payment no longer exists.";
                case 51304: return "Only a posted payment can be reversed.";
                case 1934: return "The Payments database objects are not configured with the required SQL settings. Run the latest Payments migration and try again.";
                case -2: return "The payment operation timed out. Refresh history before attempting another submission.";
                case 1205: return "Another operation changed the payment data. Refresh and try again.";
                default: return fallback;
            }
        }

        private void TryAudit(string actionName, string tableName, string recordId, string description)
        {
            try
            {
                _auditRepository.Add(CurrentSession.User == null ? (int?)null : CurrentSession.User.UserId, actionName, tableName, recordId, description);
            }
            catch
            {
                // Core payment records retain actor and timestamps even if operational audit logging is unavailable.
            }
        }

        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static string CleanOptional(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
