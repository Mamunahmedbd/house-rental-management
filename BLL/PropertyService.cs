using System;
using System.Collections.Generic;
using System.Data;
using Housing_rental.DAL;
using Housing_rental.Models;

namespace Housing_rental.BLL
{
    public class PropertyService
    {
        private readonly PropertyRepository _propertyRepository;
        private readonly AuditRepository _auditRepository;

        public PropertyService()
        {
            _propertyRepository = new PropertyRepository();
            _auditRepository = new AuditRepository();
        }

        public ServiceResult<List<Property>> SearchProperties(string searchText, bool includeInactive)
        {
            try
            {
                List<Property> properties = _propertyRepository.SearchProperties(CleanSearch(searchText), includeInactive);
                return ServiceResult<List<Property>>.Success(properties, "Properties loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Property>>.Failure("Unable to load properties. " + ex.Message);
            }
        }

        public ServiceResult<List<Property>> GetActiveProperties()
        {
            try
            {
                List<Property> properties = _propertyRepository.GetActiveProperties();
                return ServiceResult<List<Property>>.Success(properties, "Active properties loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Property>>.Failure("Unable to load active properties. " + ex.Message);
            }
        }

        public ServiceResult CreateProperty(Property property)
        {
            ServiceResult validation = ValidateProperty(property);
            if (!validation.IsSuccess)
            {
                return validation;
            }

            try
            {
                NormalizeProperty(property);

                if (_propertyRepository.PropertyNameExists(property.PropertyName, property.City, 0))
                {
                    return ServiceResult.Failure("An active property with the same name and city already exists.");
                }

                int propertyId = _propertyRepository.CreateProperty(property);
                TryAudit("Create Property", "Properties", propertyId.ToString(), "Created property '" + property.PropertyName + "'.");
                return ServiceResult.Success("Property created successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to create property. " + ex.Message);
            }
        }

        public ServiceResult UpdateProperty(Property property)
        {
            if (property == null || property.PropertyId <= 0)
            {
                return ServiceResult.Failure("Please select a valid property.");
            }

            ServiceResult validation = ValidateProperty(property);
            if (!validation.IsSuccess)
            {
                return validation;
            }

            try
            {
                NormalizeProperty(property);

                if (_propertyRepository.PropertyNameExists(property.PropertyName, property.City, property.PropertyId))
                {
                    return ServiceResult.Failure("An active property with the same name and city already exists.");
                }

                _propertyRepository.UpdateProperty(property);
                TryAudit("Update Property", "Properties", property.PropertyId.ToString(), "Updated property '" + property.PropertyName + "'.");
                return ServiceResult.Success("Property updated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to update property. " + ex.Message);
            }
        }

        public ServiceResult SetPropertyActiveStatus(int propertyId, bool isActive)
        {
            if (propertyId <= 0)
            {
                return ServiceResult.Failure("Please select a valid property.");
            }

            try
            {
                if (!isActive && _propertyRepository.PropertyHasActiveRoomsOrAgreements(propertyId))
                {
                    return ServiceResult.Failure("This property has active houses, rooms, or agreements. Close those records before deactivating it.");
                }

                _propertyRepository.SetPropertyActiveStatus(propertyId, isActive);
                TryAudit(isActive ? "Activate Property" : "Deactivate Property", "Properties", propertyId.ToString(), "Changed property active status.");
                return ServiceResult.Success(isActive ? "Property activated successfully." : "Property deactivated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to change property status. " + ex.Message);
            }
        }

        public ServiceResult<List<House>> SearchHouses(int? propertyId, string searchText, bool includeInactive)
        {
            try
            {
                List<House> houses = _propertyRepository.SearchHouses(NormalizeId(propertyId), CleanSearch(searchText), includeInactive);
                return ServiceResult<List<House>>.Success(houses, "Houses loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<List<House>>.Failure("Unable to load houses. " + ex.Message);
            }
        }

        public ServiceResult<List<House>> GetActiveHousesByPropertyId(int propertyId)
        {
            if (propertyId <= 0)
            {
                return ServiceResult<List<House>>.Success(new List<House>(), "Select a property to load houses.");
            }

            try
            {
                List<House> houses = _propertyRepository.GetActiveHousesByPropertyId(propertyId);
                return ServiceResult<List<House>>.Success(houses, "Active houses loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<List<House>>.Failure("Unable to load active houses. " + ex.Message);
            }
        }

        public ServiceResult CreateHouse(House house)
        {
            ServiceResult validation = ValidateHouse(house);
            if (!validation.IsSuccess)
            {
                return validation;
            }

            try
            {
                NormalizeHouse(house);

                if (!_propertyRepository.IsPropertyActive(house.PropertyId))
                {
                    return ServiceResult.Failure("Please select an active property.");
                }

                if (_propertyRepository.HouseNameExists(house.PropertyId, house.HouseName, 0))
                {
                    return ServiceResult.Failure("An active house with the same name already exists under this property.");
                }

                int houseId = _propertyRepository.CreateHouse(house);
                TryAudit("Create House", "Houses", houseId.ToString(), "Created house '" + house.HouseName + "'.");
                return ServiceResult.Success("House created successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to create house. " + ex.Message);
            }
        }

        public ServiceResult UpdateHouse(House house)
        {
            if (house == null || house.HouseId <= 0)
            {
                return ServiceResult.Failure("Please select a valid house.");
            }

            ServiceResult validation = ValidateHouse(house);
            if (!validation.IsSuccess)
            {
                return validation;
            }

            try
            {
                NormalizeHouse(house);

                if (!_propertyRepository.IsPropertyActive(house.PropertyId))
                {
                    return ServiceResult.Failure("Please select an active property.");
                }

                if (_propertyRepository.HouseNameExists(house.PropertyId, house.HouseName, house.HouseId))
                {
                    return ServiceResult.Failure("An active house with the same name already exists under this property.");
                }

                _propertyRepository.UpdateHouse(house);
                TryAudit("Update House", "Houses", house.HouseId.ToString(), "Updated house '" + house.HouseName + "'.");
                return ServiceResult.Success("House updated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to update house. " + ex.Message);
            }
        }

        public ServiceResult SetHouseActiveStatus(int houseId, bool isActive)
        {
            if (houseId <= 0)
            {
                return ServiceResult.Failure("Please select a valid house.");
            }

            try
            {
                if (!isActive && _propertyRepository.HouseHasActiveRoomsOrAgreements(houseId))
                {
                    return ServiceResult.Failure("This house has active rooms or agreements. Close those records before deactivating it.");
                }

                _propertyRepository.SetHouseActiveStatus(houseId, isActive);
                TryAudit(isActive ? "Activate House" : "Deactivate House", "Houses", houseId.ToString(), "Changed house active status.");
                return ServiceResult.Success(isActive ? "House activated successfully." : "House deactivated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to change house status. " + ex.Message);
            }
        }

        public ServiceResult<List<Room>> SearchRooms(int? propertyId, int? houseId, string status, string searchText)
        {
            try
            {
                List<Room> rooms = _propertyRepository.SearchRooms(NormalizeId(propertyId), NormalizeId(houseId), CleanStatusFilter(status), CleanSearch(searchText));
                return ServiceResult<List<Room>>.Success(rooms, "Rooms loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Room>>.Failure("Unable to load rooms. " + ex.Message);
            }
        }

        public ServiceResult<DataTable> GetRoomOccupancy(int? propertyId, int? houseId, string status, string searchText)
        {
            try
            {
                DataTable table = _propertyRepository.GetRoomOccupancy(NormalizeId(propertyId), NormalizeId(houseId), CleanStatusFilter(status), CleanSearch(searchText));
                return ServiceResult<DataTable>.Success(table, "Occupancy loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<DataTable>.Failure("Unable to load occupancy. " + ex.Message);
            }
        }

        public ServiceResult<List<Room>> GetAvailableRooms()
        {
            try
            {
                List<Room> rooms = _propertyRepository.GetAvailableRooms();
                return ServiceResult<List<Room>>.Success(rooms, "Available rooms loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Room>>.Failure("Unable to load available rooms. " + ex.Message);
            }
        }

        public ServiceResult CreateRoom(Room room)
        {
            ServiceResult validation = ValidateRoom(room);
            if (!validation.IsSuccess)
            {
                return validation;
            }

            try
            {
                NormalizeRoom(room);

                if (!_propertyRepository.IsHouseActive(room.HouseId))
                {
                    return ServiceResult.Failure("Please select an active house under an active property.");
                }

                if (_propertyRepository.RoomNoExists(room.HouseId, room.RoomNo, 0))
                {
                    return ServiceResult.Failure("An active room with the same number already exists under this house.");
                }

                int roomId = _propertyRepository.CreateRoom(room);
                TryAudit("Create Room", "Rooms", roomId.ToString(), "Created room '" + room.RoomNo + "'.");
                return ServiceResult.Success("Room created successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to create room. " + ex.Message);
            }
        }

        public ServiceResult UpdateRoom(Room room)
        {
            if (room == null || room.RoomId <= 0)
            {
                return ServiceResult.Failure("Please select a valid room.");
            }

            ServiceResult validation = ValidateRoom(room);
            if (!validation.IsSuccess)
            {
                return validation;
            }

            try
            {
                NormalizeRoom(room);

                if (!_propertyRepository.IsHouseActive(room.HouseId))
                {
                    return ServiceResult.Failure("Please select an active house under an active property.");
                }

                if (_propertyRepository.RoomNoExists(room.HouseId, room.RoomNo, room.RoomId))
                {
                    return ServiceResult.Failure("An active room with the same number already exists under this house.");
                }

                Room existingRoom = _propertyRepository.GetRoomById(room.RoomId);
                if (existingRoom == null)
                {
                    return ServiceResult.Failure("Selected room was not found.");
                }

                if (existingRoom.Status != room.Status && StatusRequiresNoActiveAgreement(room.Status) && _propertyRepository.RoomHasActiveAgreement(room.RoomId))
                {
                    return ServiceResult.Failure("This room has an active agreement. End the agreement before changing the room status.");
                }

                _propertyRepository.UpdateRoom(room);
                TryAudit("Update Room", "Rooms", room.RoomId.ToString(), "Updated room '" + room.RoomNo + "'.");
                return ServiceResult.Success("Room updated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to update room. " + ex.Message);
            }
        }

        public ServiceResult SetRoomStatus(int roomId, string status)
        {
            if (roomId <= 0)
            {
                return ServiceResult.Failure("Please select a valid room.");
            }

            if (!IsValidRoomStatus(status))
            {
                return ServiceResult.Failure("Please select a valid room status.");
            }

            try
            {
                Room room = _propertyRepository.GetRoomById(roomId);
                if (room == null)
                {
                    return ServiceResult.Failure("Selected room was not found.");
                }

                if (room.Status == status)
                {
                    return ServiceResult.Success("Room status is already " + status + ".");
                }

                if (StatusRequiresNoActiveAgreement(status) && _propertyRepository.RoomHasActiveAgreement(roomId))
                {
                    return ServiceResult.Failure("This room has an active agreement. End the agreement before changing the room status.");
                }

                _propertyRepository.SetRoomStatus(roomId, status);
                TryAudit("Set Room Status", "Rooms", roomId.ToString(), "Changed room '" + room.RoomNo + "' status from " + room.Status + " to " + status + ".");
                return ServiceResult.Success("Room status changed successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to change room status. " + ex.Message);
            }
        }

        public ServiceResult ValidateProperty(Property property)
        {
            if (property == null)
            {
                return ServiceResult.Failure("Property information is required.");
            }

            if (string.IsNullOrWhiteSpace(property.PropertyName))
            {
                return ServiceResult.Failure("Property name is required.");
            }

            if (property.PropertyName.Trim().Length > 100)
            {
                return ServiceResult.Failure("Property name cannot exceed 100 characters.");
            }

            return ServiceResult.Success("Property information is valid.");
        }

        public ServiceResult ValidateHouse(House house)
        {
            if (house == null)
            {
                return ServiceResult.Failure("House information is required.");
            }

            if (house.PropertyId <= 0)
            {
                return ServiceResult.Failure("A valid property is required.");
            }

            if (string.IsNullOrWhiteSpace(house.HouseName))
            {
                return ServiceResult.Failure("House name is required.");
            }

            if (house.HouseName.Trim().Length > 100)
            {
                return ServiceResult.Failure("House name cannot exceed 100 characters.");
            }

            return ServiceResult.Success("House information is valid.");
        }

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

            if (!IsValidRoomStatus(room.Status))
            {
                return ServiceResult.Failure("Please select a valid room status.");
            }

            return ServiceResult.Success("Room information is valid.");
        }

        private static void NormalizeProperty(Property property)
        {
            property.PropertyName = property.PropertyName.Trim();
            property.Address = CleanOptional(property.Address);
            property.City = CleanOptional(property.City);
            property.Description = CleanOptional(property.Description);
        }

        private static void NormalizeHouse(House house)
        {
            house.HouseName = house.HouseName.Trim();
            house.FloorNo = CleanOptional(house.FloorNo);
            house.Description = CleanOptional(house.Description);
        }

        private static void NormalizeRoom(Room room)
        {
            room.RoomNo = room.RoomNo.Trim();
            room.RoomType = CleanOptional(room.RoomType);
            room.Status = string.IsNullOrWhiteSpace(room.Status) ? "Available" : room.Status.Trim();
            room.Description = CleanOptional(room.Description);
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
            if (string.IsNullOrWhiteSpace(value) || value == "All")
            {
                return string.Empty;
            }

            return value.Trim();
        }

        private static string CleanOptional(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static bool IsValidRoomStatus(string status)
        {
            return status == "Available"
                || status == "Occupied"
                || status == "Maintenance"
                || status == "Inactive";
        }

        private static bool StatusRequiresNoActiveAgreement(string status)
        {
            return status == "Available" || status == "Maintenance" || status == "Inactive";
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
                // Audit logging should never block inventory operations.
            }
        }
    }
}
