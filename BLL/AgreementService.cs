using System;
using Housing_rental.Models;

namespace Housing_rental.BLL
{
    public class AgreementService
    {
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

            if (agreement.StartDate.Date < DateTime.Today.AddYears(-10))
            {
                return ServiceResult.Failure("Agreement start date is not realistic.");
            }

            return ServiceResult.Success("Agreement information is valid.");
        }
    }
}
