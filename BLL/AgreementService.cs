using System;
using System.Collections.Generic;
using System.Data;
using Housing_rental.DAL;
using Housing_rental.Models;

namespace Housing_rental.BLL
{
    public class AgreementService
    {
        private readonly AgreementRepository _agreementRepository;
        private readonly AuditRepository _auditRepository;
        private readonly TenantService _tenantService;
        private readonly PropertyService _propertyService;

        public AgreementService()
        {
            _agreementRepository = new AgreementRepository();
            _auditRepository = new AuditRepository();
            _tenantService = new TenantService();
            _propertyService = new PropertyService();
        }

        public ServiceResult<DataTable> GetAgreementDirectory(string searchText, string status, int? propertyId, int? tenantId, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                DataTable table = _agreementRepository.GetAgreementDirectory(
                    CleanSearch(searchText),
                    CleanStatusFilter(status),
                    NormalizeId(propertyId),
                    NormalizeId(tenantId),
                    fromDate,
                    toDate);

                return ServiceResult<DataTable>.Success(table, "Agreements loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<DataTable>.Failure("Unable to load agreements. " + ex.Message);
            }
        }

        public ServiceResult<RentalAgreement> GetAgreementById(int agreementId)
        {
            if (agreementId <= 0)
            {
                return ServiceResult<RentalAgreement>.Failure("Please select a valid agreement.");
            }

            try
            {
                RentalAgreement agreement = _agreementRepository.GetAgreementById(agreementId);
                return agreement == null
                    ? ServiceResult<RentalAgreement>.Failure("Selected agreement was not found.")
                    : ServiceResult<RentalAgreement>.Success(agreement, "Agreement loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<RentalAgreement>.Failure("Unable to load agreement. " + ex.Message);
            }
        }

        public ServiceResult<DataTable> GetAgreementDetails(int agreementId)
        {
            if (agreementId <= 0)
            {
                return ServiceResult<DataTable>.Failure("Please select a valid agreement.");
            }

            try
            {
                DataTable table = _agreementRepository.GetAgreementDetails(agreementId);
                return ServiceResult<DataTable>.Success(table, "Agreement details loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<DataTable>.Failure("Unable to load agreement details. " + ex.Message);
            }
        }

        public ServiceResult<DataTable> GetAgreementPaymentHistory(int agreementId)
        {
            if (agreementId <= 0)
            {
                return ServiceResult<DataTable>.Failure("Please select a valid agreement.");
            }

            try
            {
                DataTable table = _agreementRepository.GetAgreementPaymentHistory(agreementId);
                return ServiceResult<DataTable>.Success(table, "Agreement payments loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<DataTable>.Failure("Unable to load agreement payments. " + ex.Message);
            }
        }

        public ServiceResult<DataTable> GetAgreementBalanceSummary(int agreementId)
        {
            if (agreementId <= 0)
            {
                return ServiceResult<DataTable>.Failure("Please select a valid agreement.");
            }

            try
            {
                DataTable table = _agreementRepository.GetAgreementBalanceSummary(agreementId);
                return ServiceResult<DataTable>.Success(table, "Agreement balance loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<DataTable>.Failure("Unable to load agreement balance. " + ex.Message);
            }
        }

        public ServiceResult<DataTable> GetActiveAgreements()
        {
            try
            {
                DataTable table = _agreementRepository.GetActiveAgreements();
                return ServiceResult<DataTable>.Success(table, "Active agreements loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<DataTable>.Failure("Unable to load active agreements. " + ex.Message);
            }
        }

        public ServiceResult<DataTable> GetExpiringAgreements(int daysAhead)
        {
            if (daysAhead < 1)
            {
                daysAhead = 30;
            }

            try
            {
                DataTable table = _agreementRepository.GetExpiringAgreements(daysAhead);
                return ServiceResult<DataTable>.Success(table, "Expiring agreements loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<DataTable>.Failure("Unable to load expiring agreements. " + ex.Message);
            }
        }

        public ServiceResult<List<Tenant>> GetEligibleTenants()
        {
            return _tenantService.GetActiveTenants();
        }

        public ServiceResult<List<Room>> GetEligibleRooms()
        {
            return _propertyService.GetAvailableRooms();
        }

        public ServiceResult<string> GetNextAgreementNo(DateTime startDate)
        {
            try
            {
                string agreementNo = _agreementRepository.GetNextAgreementNo(startDate);
                return ServiceResult<string>.Success(agreementNo, "Agreement number generated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Failure("Unable to generate agreement number. " + ex.Message);
            }
        }

        public ServiceResult CreateDraftAgreement(RentalAgreement agreement)
        {
            ServiceResult validation = ValidateAgreement(agreement);
            if (!validation.IsSuccess)
            {
                return validation;
            }

            try
            {
                NormalizeAgreement(agreement);
                agreement.Status = "Draft";
                agreement.CreatedByUserId = GetCurrentUserId();
                EnsureAgreementNo(agreement);

                ServiceResult eligibility = ValidateCreateEligibility(agreement, false);
                if (!eligibility.IsSuccess)
                {
                    return eligibility;
                }

                int agreementId = _agreementRepository.CreateAgreement(agreement);
                TryAudit("Create Draft Agreement", "RentalAgreements", agreementId.ToString(), "Created draft agreement '" + agreement.AgreementNo + "'.");
                return ServiceResult.Success("Draft agreement created successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to create draft agreement. " + ex.Message);
            }
        }

        public ServiceResult CreateAndActivateAgreement(RentalAgreement agreement)
        {
            ServiceResult validation = ValidateAgreement(agreement);
            if (!validation.IsSuccess)
            {
                return validation;
            }

            try
            {
                NormalizeAgreement(agreement);
                agreement.Status = "Draft";
                agreement.CreatedByUserId = GetCurrentUserId();
                EnsureAgreementNo(agreement);

                ServiceResult eligibility = ValidateCreateEligibility(agreement, true);
                if (!eligibility.IsSuccess)
                {
                    return eligibility;
                }

                int agreementId = _agreementRepository.CreateAgreement(agreement);
                _agreementRepository.ActivateAgreement(agreementId);

                TryAudit("Create Active Agreement", "RentalAgreements", agreementId.ToString(), "Created and activated agreement '" + agreement.AgreementNo + "'.");
                return ServiceResult.Success("Agreement created and activated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to create active agreement. " + ex.Message);
            }
        }

        public ServiceResult UpdateDraftAgreement(RentalAgreement agreement)
        {
            if (agreement == null || agreement.AgreementId <= 0)
            {
                return ServiceResult.Failure("Please select a valid agreement.");
            }

            ServiceResult validation = ValidateAgreement(agreement);
            if (!validation.IsSuccess)
            {
                return validation;
            }

            try
            {
                RentalAgreement existing = _agreementRepository.GetAgreementById(agreement.AgreementId);
                if (existing == null)
                {
                    return ServiceResult.Failure("Selected agreement was not found.");
                }

                if (existing.Status != "Draft")
                {
                    return ServiceResult.Failure("Only draft agreements can be edited. Use notes or lifecycle actions for active agreements.");
                }

                NormalizeAgreement(agreement);
                agreement.Status = "Draft";
                agreement.CreatedByUserId = existing.CreatedByUserId;

                ServiceResult eligibility = ValidateCreateEligibility(agreement, false);
                if (!eligibility.IsSuccess)
                {
                    return eligibility;
                }

                _agreementRepository.UpdateDraftAgreement(agreement);
                TryAudit("Update Draft Agreement", "RentalAgreements", agreement.AgreementId.ToString(), "Updated draft agreement '" + agreement.AgreementNo + "'.");
                return ServiceResult.Success("Draft agreement updated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to update draft agreement. " + ex.Message);
            }
        }

        public ServiceResult UpdateAgreementNotes(int agreementId, string notes)
        {
            if (agreementId <= 0)
            {
                return ServiceResult.Failure("Please select a valid agreement.");
            }

            if (!string.IsNullOrWhiteSpace(notes) && notes.Trim().Length > 500)
            {
                return ServiceResult.Failure("Agreement notes cannot exceed 500 characters.");
            }

            try
            {
                RentalAgreement agreement = _agreementRepository.GetAgreementById(agreementId);
                if (agreement == null)
                {
                    return ServiceResult.Failure("Selected agreement was not found.");
                }

                _agreementRepository.UpdateAgreementNotes(agreementId, CleanOptional(notes));
                TryAudit("Update Agreement Notes", "RentalAgreements", agreementId.ToString(), "Updated notes for agreement '" + agreement.AgreementNo + "'.");
                return ServiceResult.Success("Agreement notes updated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to update agreement notes. " + ex.Message);
            }
        }

        public ServiceResult ActivateAgreement(int agreementId)
        {
            if (agreementId <= 0)
            {
                return ServiceResult.Failure("Please select a valid agreement.");
            }

            try
            {
                RentalAgreement agreement = _agreementRepository.GetAgreementById(agreementId);
                if (agreement == null)
                {
                    return ServiceResult.Failure("Selected agreement was not found.");
                }

                if (agreement.Status != "Draft")
                {
                    return ServiceResult.Failure("Only draft agreements can be activated.");
                }

                ServiceResult eligibility = ValidateCreateEligibility(agreement, true);
                if (!eligibility.IsSuccess)
                {
                    return eligibility;
                }

                _agreementRepository.ActivateAgreement(agreementId);
                TryAudit("Activate Agreement", "RentalAgreements", agreementId.ToString(), "Activated agreement '" + agreement.AgreementNo + "'.");
                return ServiceResult.Success("Agreement activated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to activate agreement. " + ex.Message);
            }
        }

        public ServiceResult TerminateAgreement(int agreementId, string reason)
        {
            return EndAgreement(agreementId, "Terminated", "Terminate Agreement", reason);
        }

        public ServiceResult ExpireAgreement(int agreementId)
        {
            return EndAgreement(agreementId, "Expired", "Expire Agreement", null);
        }

        public ServiceResult CancelAgreement(int agreementId, string reason)
        {
            if (agreementId <= 0)
            {
                return ServiceResult.Failure("Please select a valid agreement.");
            }

            try
            {
                RentalAgreement agreement = _agreementRepository.GetAgreementById(agreementId);
                if (agreement == null)
                {
                    return ServiceResult.Failure("Selected agreement was not found.");
                }

                if (agreement.Status != "Draft" && agreement.Status != "Active")
                {
                    return ServiceResult.Failure("Only draft or eligible active agreements can be cancelled.");
                }

                if (agreement.Status == "Active" && _agreementRepository.AgreementHasPayments(agreementId))
                {
                    return ServiceResult.Failure("This agreement already has payment records. Terminate it instead of cancelling it.");
                }

                if (agreement.Status == "Active" && agreement.StartDate.Date <= DateTime.Today)
                {
                    return ServiceResult.Failure("Active agreements that have already started should be terminated instead of cancelled.");
                }

                _agreementRepository.CancelAgreement(agreementId);
                TryAudit("Cancel Agreement", "RentalAgreements", agreementId.ToString(), BuildLifecycleDescription("Cancelled", agreement.AgreementNo, reason));
                return ServiceResult.Success("Agreement cancelled successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to cancel agreement. " + ex.Message);
            }
        }

        public ServiceResult ExpireDueAgreements()
        {
            try
            {
                int count = _agreementRepository.ExpireDueAgreements(DateTime.Today);
                TryAudit("Expire Due Agreements", "RentalAgreements", null, "Expired " + count + " due agreement(s).");
                return ServiceResult.Success(count == 0 ? "No due agreements needed expiry." : count + " agreement(s) expired successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to expire due agreements. " + ex.Message);
            }
        }

        public ServiceResult RenewAgreement(int agreementId, DateTime newEndDate, decimal monthlyRent, decimal securityDeposit, string notes)
        {
            if (agreementId <= 0)
            {
                return ServiceResult.Failure("Please select a valid agreement.");
            }

            if (monthlyRent <= 0)
            {
                return ServiceResult.Failure("Monthly rent must be greater than zero.");
            }

            if (securityDeposit < 0)
            {
                return ServiceResult.Failure("Security deposit cannot be negative.");
            }

            try
            {
                RentalAgreement source = _agreementRepository.GetAgreementById(agreementId);
                if (source == null)
                {
                    return ServiceResult.Failure("Selected agreement was not found.");
                }

                if (source.Status != "Active")
                {
                    return ServiceResult.Failure("Only active agreements can be renewed.");
                }

                DateTime newStartDate = source.EndDate.Date.AddDays(1);
                if (newEndDate.Date <= newStartDate)
                {
                    return ServiceResult.Failure("Renewal end date must be after the renewal start date.");
                }

                RentalAgreement renewal = new RentalAgreement
                {
                    AgreementNo = _agreementRepository.GetNextAgreementNo(newStartDate),
                    TenantId = source.TenantId,
                    RoomId = source.RoomId,
                    StartDate = newStartDate,
                    EndDate = newEndDate.Date,
                    MonthlyRent = monthlyRent,
                    SecurityDeposit = securityDeposit,
                    Status = "Active",
                    Notes = CleanOptional(notes),
                    CreatedByUserId = GetCurrentUserId()
                };

                int newAgreementId = _agreementRepository.RenewAgreement(agreementId, renewal);
                TryAudit("Renew Agreement", "RentalAgreements", newAgreementId.ToString(), "Renewed agreement '" + source.AgreementNo + "' as '" + renewal.AgreementNo + "'.");
                return ServiceResult.Success("Agreement renewed successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to renew agreement. " + ex.Message);
            }
        }

        public ServiceResult ValidateAgreement(RentalAgreement agreement)
        {
            if (agreement == null)
            {
                return ServiceResult.Failure("Agreement information is required.");
            }

            if (agreement.TenantId <= 0)
            {
                return ServiceResult.Failure("A valid tenant is required.");
            }

            if (agreement.RoomId <= 0)
            {
                return ServiceResult.Failure("A valid room is required.");
            }

            if (agreement.StartDate.Date >= agreement.EndDate.Date)
            {
                return ServiceResult.Failure("Agreement end date must be after start date.");
            }

            if (agreement.MonthlyRent <= 0)
            {
                return ServiceResult.Failure("Monthly rent must be greater than zero.");
            }

            if (agreement.SecurityDeposit < 0)
            {
                return ServiceResult.Failure("Security deposit cannot be negative.");
            }

            if (!string.IsNullOrWhiteSpace(agreement.AgreementNo) && agreement.AgreementNo.Trim().Length > 50)
            {
                return ServiceResult.Failure("Agreement number cannot exceed 50 characters.");
            }

            if (!string.IsNullOrWhiteSpace(agreement.Notes) && agreement.Notes.Trim().Length > 500)
            {
                return ServiceResult.Failure("Agreement notes cannot exceed 500 characters.");
            }

            if (!string.IsNullOrWhiteSpace(agreement.Status) && !IsValidAgreementStatus(agreement.Status.Trim()))
            {
                return ServiceResult.Failure("Please select a valid agreement status.");
            }

            if (agreement.StartDate.Date < DateTime.Today.AddYears(-10))
            {
                return ServiceResult.Failure("Agreement start date is not realistic.");
            }

            return ServiceResult.Success("Agreement information is valid.");
        }

        private ServiceResult EndAgreement(int agreementId, string newStatus, string auditAction, string reason)
        {
            if (agreementId <= 0)
            {
                return ServiceResult.Failure("Please select a valid agreement.");
            }

            try
            {
                RentalAgreement agreement = _agreementRepository.GetAgreementById(agreementId);
                if (agreement == null)
                {
                    return ServiceResult.Failure("Selected agreement was not found.");
                }

                if (agreement.Status != "Active")
                {
                    return ServiceResult.Failure("Only active agreements can be " + newStatus.ToLowerInvariant() + ".");
                }

                if (newStatus == "Expired" && agreement.EndDate.Date >= DateTime.Today)
                {
                    return ServiceResult.Failure("Only agreements with an end date before today can be expired.");
                }

                if (newStatus == "Terminated")
                {
                    _agreementRepository.TerminateAgreement(agreementId);
                }
                else
                {
                    _agreementRepository.ExpireAgreement(agreementId);
                }

                TryAudit(auditAction, "RentalAgreements", agreementId.ToString(), BuildLifecycleDescription(newStatus, agreement.AgreementNo, reason));
                return ServiceResult.Success("Agreement " + newStatus.ToLowerInvariant() + " successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to " + newStatus.ToLowerInvariant() + " agreement. " + ex.Message);
            }
        }

        private ServiceResult ValidateCreateEligibility(RentalAgreement agreement, bool requireAvailableRoom)
        {
            if (_agreementRepository.AgreementNoExists(agreement.AgreementNo, agreement.AgreementId))
            {
                return ServiceResult.Failure("An agreement with the same agreement number already exists.");
            }

            if (!_agreementRepository.TenantIsActive(agreement.TenantId))
            {
                return ServiceResult.Failure("Please select an active tenant.");
            }

            if (requireAvailableRoom && !_agreementRepository.RoomIsAvailable(agreement.RoomId))
            {
                return ServiceResult.Failure("Please select an available room.");
            }

            if (_agreementRepository.RoomHasActiveAgreement(agreement.RoomId, agreement.AgreementId))
            {
                return ServiceResult.Failure("This room already has an active agreement.");
            }

            if (_agreementRepository.TenantHasActiveAgreement(agreement.TenantId, agreement.AgreementId))
            {
                return ServiceResult.Failure("This tenant already has an active agreement.");
            }

            return ServiceResult.Success("Agreement can be saved.");
        }

        private void EnsureAgreementNo(RentalAgreement agreement)
        {
            if (string.IsNullOrWhiteSpace(agreement.AgreementNo))
            {
                agreement.AgreementNo = _agreementRepository.GetNextAgreementNo(agreement.StartDate);
            }
            else
            {
                agreement.AgreementNo = agreement.AgreementNo.Trim();
            }
        }

        private static void NormalizeAgreement(RentalAgreement agreement)
        {
            agreement.AgreementNo = CleanOptional(agreement.AgreementNo);
            agreement.StartDate = agreement.StartDate.Date;
            agreement.EndDate = agreement.EndDate.Date;
            agreement.Status = string.IsNullOrWhiteSpace(agreement.Status) ? "Draft" : agreement.Status.Trim();
            agreement.Notes = CleanOptional(agreement.Notes);
        }

        private static int GetCurrentUserId()
        {
            return CurrentSession.User == null ? 1 : CurrentSession.User.UserId;
        }

        private static int? NormalizeId(int? value)
        {
            return value.HasValue && value.Value > 0 ? value : null;
        }

        private static string CleanSearch(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static string CleanStatusFilter(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "All statuses")
            {
                return string.Empty;
            }

            return value.Trim();
        }

        private static string CleanOptional(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static bool IsValidAgreementStatus(string status)
        {
            return status == "Draft"
                || status == "Active"
                || status == "Expired"
                || status == "Terminated"
                || status == "Cancelled";
        }

        private static string BuildLifecycleDescription(string status, string agreementNo, string reason)
        {
            string description = status + " agreement '" + agreementNo + "'.";
            if (!string.IsNullOrWhiteSpace(reason))
            {
                description += " Reason: " + reason.Trim();
            }

            return description;
        }

        private void TryAudit(string actionName, string tableName, string recordId, string description)
        {
            try
            {
                int? userId = CurrentSession.User == null ? (int?)null : CurrentSession.User.UserId;
                _auditRepository.Add(userId, actionName, tableName, recordId, description);
            }
            catch
            {
                // Audit logging should never block agreement operations.
            }
        }
    }
}
