using Housing_rental.Models;

namespace Housing_rental.BLL
{
    public class PropertyService
    {
        public ServiceResult ValidateRoom(Room room)
        {
            if (room == null)
            {
                return ServiceResult.Failure("Room information is required.");
            }

            if (string.IsNullOrWhiteSpace(room.RoomNo))
            {
                return ServiceResult.Failure("Room number is required.");
            }

            if (room.HouseId <= 0)
            {
                return ServiceResult.Failure("A valid house is required.");
            }

            if (room.MonthlyRent <= 0)
            {
                return ServiceResult.Failure("Monthly rent must be greater than zero.");
            }

            return ServiceResult.Success("Room information is valid.");
        }
    }
}
