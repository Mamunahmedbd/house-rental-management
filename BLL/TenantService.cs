using Housing_rental.Models;

namespace Housing_rental.BLL
{
    public class TenantService
    {
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

            return ServiceResult.Success("Tenant information is valid.");
        }
    }
}
