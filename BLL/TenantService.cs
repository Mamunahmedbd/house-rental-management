using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using Housing_rental.DAL;
using Housing_rental.Models;

namespace Housing_rental.BLL
{
    public class TenantService
    {
        private readonly TenantRepository _tenantRepository;
        private readonly AuditRepository _auditRepository;

        public TenantService()
        {
            _tenantRepository = new TenantRepository();
            _auditRepository = new AuditRepository();
        }

        public ServiceResult<List<Tenant>> SearchTenants(string searchText, string status, bool includeInactive)
        {
            try
            {
                List<Tenant> tenants = _tenantRepository.SearchTenants(CleanSearch(searchText), CleanStatusFilter(status), includeInactive);
                return ServiceResult<List<Tenant>>.Success(tenants, "Tenants loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Tenant>>.Failure("Unable to load tenants. " + ex.Message);
            }
        }

        public ServiceResult<List<Tenant>> GetActiveTenants()
        {
            try
            {
                List<Tenant> tenants = _tenantRepository.GetActiveTenants();
                return ServiceResult<List<Tenant>>.Success(tenants, "Active tenants loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Tenant>>.Failure("Unable to load active tenants. " + ex.Message);
            }
        }

        public ServiceResult<Tenant> GetTenantById(int tenantId)
        {
            if (tenantId <= 0)
            {
                return ServiceResult<Tenant>.Failure("Please select a valid tenant.");
            }

            try
            {
                Tenant tenant = _tenantRepository.GetTenantById(tenantId);
                return tenant == null
                    ? ServiceResult<Tenant>.Failure("Selected tenant was not found.")
                    : ServiceResult<Tenant>.Success(tenant, "Tenant loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<Tenant>.Failure("Unable to load tenant. " + ex.Message);
            }
        }

        public ServiceResult CreateTenant(Tenant tenant)
        {
            ServiceResult validation = ValidateTenant(tenant);
            if (!validation.IsSuccess)
            {
                return validation;
            }

            try
            {
                NormalizeTenant(tenant);

                if (tenant.Status == "Blacklisted" && !CurrentSession.IsAdmin)
                {
                    return ServiceResult.Failure("Only an Admin can create a blacklisted tenant.");
                }

                if (_tenantRepository.NationalIdExists(tenant.NationalId, 0))
                {
                    return ServiceResult.Failure("A tenant with the same national ID already exists.");
                }

                int tenantId = _tenantRepository.CreateTenant(tenant);
                TryAudit("Create Tenant", "Tenants", tenantId.ToString(), "Created tenant '" + tenant.FullName + "'.");
                return ServiceResult.Success("Tenant created successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to create tenant. " + ex.Message);
            }
        }

        public ServiceResult UpdateTenant(Tenant tenant)
        {
            if (tenant == null || tenant.TenantId <= 0)
            {
                return ServiceResult.Failure("Please select a valid tenant.");
            }

            ServiceResult validation = ValidateTenant(tenant);
            if (!validation.IsSuccess)
            {
                return validation;
            }

            try
            {
                NormalizeTenant(tenant);

                Tenant existingTenant = _tenantRepository.GetTenantById(tenant.TenantId);
                if (existingTenant == null)
                {
                    return ServiceResult.Failure("Selected tenant was not found.");
                }

                if (_tenantRepository.NationalIdExists(tenant.NationalId, tenant.TenantId))
                {
                    return ServiceResult.Failure("A tenant with the same national ID already exists.");
                }

                if (existingTenant.Status != tenant.Status)
                {
                    ServiceResult statusResult = CanChangeStatus(existingTenant, tenant.Status);
                    if (!statusResult.IsSuccess)
                    {
                        return statusResult;
                    }
                }

                _tenantRepository.UpdateTenant(tenant);
                TryAudit("Update Tenant", "Tenants", tenant.TenantId.ToString(), "Updated tenant '" + tenant.FullName + "'.");
                return ServiceResult.Success("Tenant updated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to update tenant. " + ex.Message);
            }
        }

        public ServiceResult SetTenantStatus(int tenantId, string status)
        {
            if (tenantId <= 0)
            {
                return ServiceResult.Failure("Please select a valid tenant.");
            }

            if (!IsValidTenantStatus(status))
            {
                return ServiceResult.Failure("Please select a valid tenant status.");
            }

            try
            {
                Tenant tenant = _tenantRepository.GetTenantById(tenantId);
                if (tenant == null)
                {
                    return ServiceResult.Failure("Selected tenant was not found.");
                }

                if (tenant.Status == status)
                {
                    return ServiceResult.Success("Tenant status is already " + status + ".");
                }

                ServiceResult statusResult = CanChangeStatus(tenant, status);
                if (!statusResult.IsSuccess)
                {
                    return statusResult;
                }

                _tenantRepository.SetTenantStatus(tenantId, status);
                TryAudit(GetStatusActionName(status), "Tenants", tenantId.ToString(), "Changed tenant '" + tenant.FullName + "' status from " + tenant.Status + " to " + status + ".");
                return ServiceResult.Success("Tenant status changed successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to change tenant status. " + ex.Message);
            }
        }

        public ServiceResult<DataTable> GetTenantAgreementHistory(int tenantId)
        {
            if (tenantId <= 0)
            {
                return ServiceResult<DataTable>.Failure("Please select a valid tenant.");
            }

            try
            {
                DataTable table = _tenantRepository.GetTenantAgreementHistory(tenantId);
                return ServiceResult<DataTable>.Success(table, "Agreement history loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<DataTable>.Failure("Unable to load agreement history. " + ex.Message);
            }
        }

        public ServiceResult<DataTable> GetTenantPaymentHistory(int tenantId)
        {
            if (tenantId <= 0)
            {
                return ServiceResult<DataTable>.Failure("Please select a valid tenant.");
            }

            try
            {
                DataTable table = _tenantRepository.GetTenantPaymentHistory(tenantId);
                return ServiceResult<DataTable>.Success(table, "Payment history loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<DataTable>.Failure("Unable to load payment history. " + ex.Message);
            }
        }

        public ServiceResult<DataTable> GetTenantCurrentOccupancy(int tenantId)
        {
            if (tenantId <= 0)
            {
                return ServiceResult<DataTable>.Failure("Please select a valid tenant.");
            }

            try
            {
                DataTable table = _tenantRepository.GetTenantCurrentOccupancy(tenantId);
                return ServiceResult<DataTable>.Success(table, "Current occupancy loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<DataTable>.Failure("Unable to load current occupancy. " + ex.Message);
            }
        }

        public ServiceResult<DataTable> GetTenantBalanceSummary(int tenantId)
        {
            if (tenantId <= 0)
            {
                return ServiceResult<DataTable>.Failure("Please select a valid tenant.");
            }

            try
            {
                DataTable table = _tenantRepository.GetTenantBalanceSummary(tenantId);
                return ServiceResult<DataTable>.Success(table, "Tenant balance loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<DataTable>.Failure("Unable to load tenant balance. " + ex.Message);
            }
        }

        public ServiceResult<DataTable> GetTenantDirectory(string searchText, string status)
        {
            try
            {
                DataTable table = _tenantRepository.GetTenantDirectory(CleanSearch(searchText), CleanStatusFilter(status));
                return ServiceResult<DataTable>.Success(table, "Tenant directory loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<DataTable>.Failure("Unable to load tenant directory. " + ex.Message);
            }
        }

        public ServiceResult ValidateTenant(Tenant tenant)
        {
            if (tenant == null)
            {
                return ServiceResult.Failure("Tenant information is required.");
            }

            if (string.IsNullOrWhiteSpace(tenant.FullName))
            {
                return ServiceResult.Failure("Tenant name is required.");
            }

            if (string.IsNullOrWhiteSpace(tenant.Phone))
            {
                return ServiceResult.Failure("Tenant phone number is required.");
            }

            if (tenant.FullName.Trim().Length > 120)
            {
                return ServiceResult.Failure("Tenant name cannot exceed 120 characters.");
            }

            if (tenant.Phone.Trim().Length > 30)
            {
                return ServiceResult.Failure("Tenant phone number cannot exceed 30 characters.");
            }

            if (!string.IsNullOrWhiteSpace(tenant.Email))
            {
                if (tenant.Email.Trim().Length > 100)
                {
                    return ServiceResult.Failure("Tenant email cannot exceed 100 characters.");
                }

                if (!IsValidEmail(tenant.Email.Trim()))
                {
                    return ServiceResult.Failure("Please enter a valid tenant email address.");
                }
            }

            if (!string.IsNullOrWhiteSpace(tenant.NationalId) && tenant.NationalId.Trim().Length > 80)
            {
                return ServiceResult.Failure("National ID cannot exceed 80 characters.");
            }

            if (!string.IsNullOrWhiteSpace(tenant.Address) && tenant.Address.Trim().Length > 250)
            {
                return ServiceResult.Failure("Address cannot exceed 250 characters.");
            }

            if (!string.IsNullOrWhiteSpace(tenant.EmergencyContactName) && tenant.EmergencyContactName.Trim().Length > 100)
            {
                return ServiceResult.Failure("Emergency contact name cannot exceed 100 characters.");
            }

            if (!string.IsNullOrWhiteSpace(tenant.EmergencyContactPhone) && tenant.EmergencyContactPhone.Trim().Length > 30)
            {
                return ServiceResult.Failure("Emergency contact phone cannot exceed 30 characters.");
            }

            if (!string.IsNullOrWhiteSpace(tenant.Status) && !IsValidTenantStatus(tenant.Status.Trim()))
            {
                return ServiceResult.Failure("Please select a valid tenant status.");
            }

            return ServiceResult.Success("Tenant information is valid.");
        }

        private ServiceResult CanChangeStatus(Tenant tenant, string newStatus)
        {
            if (!IsValidTenantStatus(newStatus))
            {
                return ServiceResult.Failure("Please select a valid tenant status.");
            }

            if ((newStatus == "Inactive" || newStatus == "Blacklisted") && _tenantRepository.TenantHasActiveAgreement(tenant.TenantId))
            {
                return ServiceResult.Failure("This tenant has an active agreement. End the agreement before changing the tenant status.");
            }

            if (newStatus == "Blacklisted" && !CurrentSession.IsAdmin)
            {
                return ServiceResult.Failure("Only an Admin can blacklist a tenant.");
            }

            if (tenant.Status == "Blacklisted" && newStatus != "Blacklisted" && !CurrentSession.IsAdmin)
            {
                return ServiceResult.Failure("Only an Admin can reactivate a blacklisted tenant.");
            }

            return ServiceResult.Success("Tenant status can be changed.");
        }

        private static void NormalizeTenant(Tenant tenant)
        {
            tenant.FullName = tenant.FullName.Trim();
            tenant.Phone = tenant.Phone.Trim();
            tenant.Email = CleanOptional(tenant.Email);
            if (tenant.Email != null)
            {
                tenant.Email = tenant.Email.ToLowerInvariant();
            }

            tenant.NationalId = CleanOptional(tenant.NationalId);
            tenant.Address = CleanOptional(tenant.Address);
            tenant.EmergencyContactName = CleanOptional(tenant.EmergencyContactName);
            tenant.EmergencyContactPhone = CleanOptional(tenant.EmergencyContactPhone);
            tenant.Status = string.IsNullOrWhiteSpace(tenant.Status) ? "Active" : tenant.Status.Trim();
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

        private static bool IsValidTenantStatus(string status)
        {
            return status == "Active"
                || status == "Inactive"
                || status == "Blacklisted";
        }

        private static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private static string GetStatusActionName(string status)
        {
            if (status == "Active")
            {
                return "Activate Tenant";
            }

            if (status == "Blacklisted")
            {
                return "Blacklist Tenant";
            }

            return "Deactivate Tenant";
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
                // Audit logging should never block tenant operations.
            }
        }
    }
}
