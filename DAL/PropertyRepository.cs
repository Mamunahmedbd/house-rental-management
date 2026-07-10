using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Housing_rental.Models;

namespace Housing_rental.DAL
{
    public class PropertyRepository
    {
        public List<Property> SearchProperties(string searchText, bool includeInactive)
        {
            const string sql = @"
SELECT
    PropertyId,
    PropertyName,
    Address,
    City,
    Description,
    IsActive,
    CreatedAt
FROM dbo.Properties
WHERE
    (@IncludeInactive = 1 OR IsActive = 1)
    AND
    (
        @SearchText = ''
        OR PropertyName LIKE '%' + @SearchText + '%'
        OR ISNULL(Address, '') LIKE '%' + @SearchText + '%'
        OR ISNULL(City, '') LIKE '%' + @SearchText + '%'
    )
ORDER BY IsActive DESC, PropertyName ASC;";

            List<Property> properties = new List<Property>();

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@SearchText", searchText ?? string.Empty);
                command.Parameters.AddWithValue("@IncludeInactive", includeInactive);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        properties.Add(MapProperty(reader));
                    }
                }
            }

            return properties;
        }

        public List<Property> GetActiveProperties()
        {
            return SearchProperties(string.Empty, false);
        }

        public Property GetPropertyById(int propertyId)
        {
            const string sql = @"
SELECT
    PropertyId,
    PropertyName,
    Address,
    City,
    Description,
    IsActive,
    CreatedAt
FROM dbo.Properties
WHERE PropertyId = @PropertyId;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@PropertyId", propertyId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? MapProperty(reader) : null;
                }
            }
        }

        public bool IsPropertyActive(int propertyId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Properties WHERE PropertyId = @PropertyId AND IsActive = 1;";
            return Exists(sql, SqlHelper.Parameter("@PropertyId", propertyId));
        }

        public bool PropertyNameExists(string propertyName, string city, int excludedPropertyId)
        {
            const string sql = @"
SELECT COUNT(1)
FROM dbo.Properties
WHERE
    IsActive = 1
    AND PropertyId <> @ExcludedPropertyId
    AND LOWER(PropertyName) = LOWER(@PropertyName)
    AND ISNULL(LOWER(City), '') = ISNULL(LOWER(@City), '');";

            return Exists(
                sql,
                SqlHelper.Parameter("@PropertyName", propertyName),
                SqlHelper.Parameter("@City", city),
                SqlHelper.Parameter("@ExcludedPropertyId", excludedPropertyId));
        }

        public int CreateProperty(Property property)
        {
            const string sql = @"
INSERT INTO dbo.Properties
(
    PropertyName,
    Address,
    City,
    Description,
    IsActive,
    CreatedAt
)
OUTPUT INSERTED.PropertyId
VALUES
(
    @PropertyName,
    @Address,
    @City,
    @Description,
    @IsActive,
    GETDATE()
);";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                AddPropertyParameters(command, property);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void UpdateProperty(Property property)
        {
            const string sql = @"
UPDATE dbo.Properties
SET
    PropertyName = @PropertyName,
    Address = @Address,
    City = @City,
    Description = @Description,
    IsActive = @IsActive
WHERE PropertyId = @PropertyId;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@PropertyId", property.PropertyId);
                AddPropertyParameters(command, property);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void SetPropertyActiveStatus(int propertyId, bool isActive)
        {
            const string sql = "UPDATE dbo.Properties SET IsActive = @IsActive WHERE PropertyId = @PropertyId;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@PropertyId", propertyId);
                command.Parameters.AddWithValue("@IsActive", isActive);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public bool PropertyHasActiveRoomsOrAgreements(int propertyId)
        {
            const string sql = @"
SELECT COUNT(1)
FROM dbo.Properties p
INNER JOIN dbo.Houses h ON h.PropertyId = p.PropertyId
INNER JOIN dbo.Rooms r ON r.HouseId = h.HouseId
LEFT JOIN dbo.RentalAgreements a ON a.RoomId = r.RoomId AND a.Status = 'Active'
WHERE
    p.PropertyId = @PropertyId
    AND
    (
        h.IsActive = 1
        OR r.Status IN ('Available', 'Occupied', 'Maintenance')
        OR a.AgreementId IS NOT NULL
    );";

            return Exists(sql, SqlHelper.Parameter("@PropertyId", propertyId));
        }

        public List<House> SearchHouses(int? propertyId, string searchText, bool includeInactive)
        {
            const string sql = @"
SELECT
    HouseId,
    PropertyId,
    HouseName,
    FloorNo,
    Description,
    IsActive,
    CreatedAt
FROM dbo.Houses
WHERE
    (@PropertyId IS NULL OR PropertyId = @PropertyId)
    AND (@IncludeInactive = 1 OR IsActive = 1)
    AND
    (
        @SearchText = ''
        OR HouseName LIKE '%' + @SearchText + '%'
        OR ISNULL(FloorNo, '') LIKE '%' + @SearchText + '%'
    )
ORDER BY IsActive DESC, HouseName ASC;";

            List<House> houses = new List<House>();

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(SqlHelper.Parameter("@PropertyId", propertyId));
                command.Parameters.AddWithValue("@SearchText", searchText ?? string.Empty);
                command.Parameters.AddWithValue("@IncludeInactive", includeInactive);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        houses.Add(MapHouse(reader));
                    }
                }
            }

            return houses;
        }

        public List<House> GetActiveHousesByPropertyId(int propertyId)
        {
            return SearchHouses(propertyId, string.Empty, false);
        }

        public House GetHouseById(int houseId)
        {
            const string sql = @"
SELECT
    HouseId,
    PropertyId,
    HouseName,
    FloorNo,
    Description,
    IsActive,
    CreatedAt
FROM dbo.Houses
WHERE HouseId = @HouseId;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@HouseId", houseId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? MapHouse(reader) : null;
                }
            }
        }

        public bool IsHouseActive(int houseId)
        {
            const string sql = @"
SELECT COUNT(1)
FROM dbo.Houses h
INNER JOIN dbo.Properties p ON p.PropertyId = h.PropertyId
WHERE h.HouseId = @HouseId AND h.IsActive = 1 AND p.IsActive = 1;";

            return Exists(sql, SqlHelper.Parameter("@HouseId", houseId));
        }

        public bool HouseNameExists(int propertyId, string houseName, int excludedHouseId)
        {
            const string sql = @"
SELECT COUNT(1)
FROM dbo.Houses
WHERE
    IsActive = 1
    AND PropertyId = @PropertyId
    AND HouseId <> @ExcludedHouseId
    AND LOWER(HouseName) = LOWER(@HouseName);";

            return Exists(
                sql,
                SqlHelper.Parameter("@PropertyId", propertyId),
                SqlHelper.Parameter("@HouseName", houseName),
                SqlHelper.Parameter("@ExcludedHouseId", excludedHouseId));
        }

        public int CreateHouse(House house)
        {
            const string sql = @"
INSERT INTO dbo.Houses
(
    PropertyId,
    HouseName,
    FloorNo,
    Description,
    IsActive,
    CreatedAt
)
OUTPUT INSERTED.HouseId
VALUES
(
    @PropertyId,
    @HouseName,
    @FloorNo,
    @Description,
    @IsActive,
    GETDATE()
);";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                AddHouseParameters(command, house);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void UpdateHouse(House house)
        {
            const string sql = @"
UPDATE dbo.Houses
SET
    PropertyId = @PropertyId,
    HouseName = @HouseName,
    FloorNo = @FloorNo,
    Description = @Description,
    IsActive = @IsActive
WHERE HouseId = @HouseId;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@HouseId", house.HouseId);
                AddHouseParameters(command, house);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void SetHouseActiveStatus(int houseId, bool isActive)
        {
            const string sql = "UPDATE dbo.Houses SET IsActive = @IsActive WHERE HouseId = @HouseId;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@HouseId", houseId);
                command.Parameters.AddWithValue("@IsActive", isActive);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public bool HouseHasActiveRoomsOrAgreements(int houseId)
        {
            const string sql = @"
SELECT COUNT(1)
FROM dbo.Rooms r
LEFT JOIN dbo.RentalAgreements a ON a.RoomId = r.RoomId AND a.Status = 'Active'
WHERE
    r.HouseId = @HouseId
    AND
    (
        r.Status IN ('Available', 'Occupied', 'Maintenance')
        OR a.AgreementId IS NOT NULL
    );";

            return Exists(sql, SqlHelper.Parameter("@HouseId", houseId));
        }

        public List<Room> SearchRooms(int? propertyId, int? houseId, string status, string searchText)
        {
            const string sql = @"
SELECT
    r.RoomId,
    r.HouseId,
    r.RoomNo,
    r.RoomType,
    r.MonthlyRent,
    r.Status,
    r.Description,
    r.CreatedAt
FROM dbo.Rooms r
INNER JOIN dbo.Houses h ON h.HouseId = r.HouseId
WHERE
    (@PropertyId IS NULL OR h.PropertyId = @PropertyId)
    AND (@HouseId IS NULL OR r.HouseId = @HouseId)
    AND (@Status = '' OR r.Status = @Status)
    AND
    (
        @SearchText = ''
        OR r.RoomNo LIKE '%' + @SearchText + '%'
        OR ISNULL(r.RoomType, '') LIKE '%' + @SearchText + '%'
    )
ORDER BY
    CASE r.Status
        WHEN 'Available' THEN 1
        WHEN 'Occupied' THEN 2
        WHEN 'Maintenance' THEN 3
        ELSE 4
    END,
    r.RoomNo ASC;";

            List<Room> rooms = new List<Room>();

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(SqlHelper.Parameter("@PropertyId", propertyId));
                command.Parameters.Add(SqlHelper.Parameter("@HouseId", houseId));
                command.Parameters.AddWithValue("@Status", status ?? string.Empty);
                command.Parameters.AddWithValue("@SearchText", searchText ?? string.Empty);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rooms.Add(MapRoom(reader));
                    }
                }
            }

            return rooms;
        }

        public DataTable GetRoomOccupancy(int? propertyId, int? houseId, string status, string searchText)
        {
            const string sql = @"
SELECT
    PropertyName,
    HouseName,
    RoomNo,
    RoomType,
    MonthlyRent,
    RoomStatus,
    TenantName,
    AgreementNo,
    StartDate,
    EndDate,
    AgreementStatus
FROM dbo.vw_RoomOccupancy
WHERE
    (@PropertyId IS NULL OR PropertyId = @PropertyId)
    AND (@HouseId IS NULL OR HouseId = @HouseId)
    AND (@Status = '' OR RoomStatus = @Status)
    AND
    (
        @SearchText = ''
        OR PropertyName LIKE '%' + @SearchText + '%'
        OR HouseName LIKE '%' + @SearchText + '%'
        OR RoomNo LIKE '%' + @SearchText + '%'
        OR ISNULL(TenantName, '') LIKE '%' + @SearchText + '%'
    )
ORDER BY PropertyName, HouseName, RoomNo;";

            return SqlHelper.ExecuteDataTable(
                sql,
                SqlHelper.Parameter("@PropertyId", propertyId),
                SqlHelper.Parameter("@HouseId", houseId),
                SqlHelper.Parameter("@Status", status ?? string.Empty),
                SqlHelper.Parameter("@SearchText", searchText ?? string.Empty));
        }

        public List<Room> GetAvailableRooms()
        {
            return SearchRooms(null, null, "Available", string.Empty);
        }

        public Room GetRoomById(int roomId)
        {
            const string sql = @"
SELECT
    RoomId,
    HouseId,
    RoomNo,
    RoomType,
    MonthlyRent,
    Status,
    Description,
    CreatedAt
FROM dbo.Rooms
WHERE RoomId = @RoomId;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@RoomId", roomId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? MapRoom(reader) : null;
                }
            }
        }

        public bool RoomNoExists(int houseId, string roomNo, int excludedRoomId)
        {
            const string sql = @"
SELECT COUNT(1)
FROM dbo.Rooms
WHERE
    Status <> 'Inactive'
    AND HouseId = @HouseId
    AND RoomId <> @ExcludedRoomId
    AND LOWER(RoomNo) = LOWER(@RoomNo);";

            return Exists(
                sql,
                SqlHelper.Parameter("@HouseId", houseId),
                SqlHelper.Parameter("@RoomNo", roomNo),
                SqlHelper.Parameter("@ExcludedRoomId", excludedRoomId));
        }

        public bool RoomHasActiveAgreement(int roomId)
        {
            const string sql = @"
SELECT COUNT(1)
FROM dbo.RentalAgreements
WHERE RoomId = @RoomId AND Status = 'Active';";

            return Exists(sql, SqlHelper.Parameter("@RoomId", roomId));
        }

        public int CreateRoom(Room room)
        {
            const string sql = @"
INSERT INTO dbo.Rooms
(
    HouseId,
    RoomNo,
    RoomType,
    MonthlyRent,
    Status,
    Description,
    CreatedAt
)
OUTPUT INSERTED.RoomId
VALUES
(
    @HouseId,
    @RoomNo,
    @RoomType,
    @MonthlyRent,
    @Status,
    @Description,
    GETDATE()
);";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                AddRoomParameters(command, room);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void UpdateRoom(Room room)
        {
            const string sql = @"
UPDATE dbo.Rooms
SET
    HouseId = @HouseId,
    RoomNo = @RoomNo,
    RoomType = @RoomType,
    MonthlyRent = @MonthlyRent,
    Status = @Status,
    Description = @Description
WHERE RoomId = @RoomId;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@RoomId", room.RoomId);
                AddRoomParameters(command, room);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void SetRoomStatus(int roomId, string status)
        {
            const string sql = "UPDATE dbo.Rooms SET Status = @Status WHERE RoomId = @RoomId;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@RoomId", roomId);
                command.Parameters.AddWithValue("@Status", status);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static bool Exists(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private static void AddPropertyParameters(SqlCommand command, Property property)
        {
            command.Parameters.AddWithValue("@PropertyName", property.PropertyName);
            command.Parameters.Add(SqlHelper.Parameter("@Address", property.Address));
            command.Parameters.Add(SqlHelper.Parameter("@City", property.City));
            command.Parameters.Add(SqlHelper.Parameter("@Description", property.Description));
            command.Parameters.AddWithValue("@IsActive", property.IsActive);
        }

        private static void AddHouseParameters(SqlCommand command, House house)
        {
            command.Parameters.AddWithValue("@PropertyId", house.PropertyId);
            command.Parameters.AddWithValue("@HouseName", house.HouseName);
            command.Parameters.Add(SqlHelper.Parameter("@FloorNo", house.FloorNo));
            command.Parameters.Add(SqlHelper.Parameter("@Description", house.Description));
            command.Parameters.AddWithValue("@IsActive", house.IsActive);
        }

        private static void AddRoomParameters(SqlCommand command, Room room)
        {
            command.Parameters.AddWithValue("@HouseId", room.HouseId);
            command.Parameters.AddWithValue("@RoomNo", room.RoomNo);
            command.Parameters.Add(SqlHelper.Parameter("@RoomType", room.RoomType));
            command.Parameters.AddWithValue("@MonthlyRent", room.MonthlyRent);
            command.Parameters.AddWithValue("@Status", room.Status);
            command.Parameters.Add(SqlHelper.Parameter("@Description", room.Description));
        }

        private static Property MapProperty(SqlDataReader reader)
        {
            return new Property
            {
                PropertyId = Convert.ToInt32(reader["PropertyId"]),
                PropertyName = Convert.ToString(reader["PropertyName"]),
                Address = Convert.ToString(reader["Address"]),
                City = Convert.ToString(reader["City"]),
                Description = Convert.ToString(reader["Description"]),
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            };
        }

        private static House MapHouse(SqlDataReader reader)
        {
            return new House
            {
                HouseId = Convert.ToInt32(reader["HouseId"]),
                PropertyId = Convert.ToInt32(reader["PropertyId"]),
                HouseName = Convert.ToString(reader["HouseName"]),
                FloorNo = Convert.ToString(reader["FloorNo"]),
                Description = Convert.ToString(reader["Description"]),
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            };
        }

        private static Room MapRoom(SqlDataReader reader)
        {
            return new Room
            {
                RoomId = Convert.ToInt32(reader["RoomId"]),
                HouseId = Convert.ToInt32(reader["HouseId"]),
                RoomNo = Convert.ToString(reader["RoomNo"]),
                RoomType = Convert.ToString(reader["RoomType"]),
                MonthlyRent = Convert.ToDecimal(reader["MonthlyRent"]),
                Status = Convert.ToString(reader["Status"]),
                Description = Convert.ToString(reader["Description"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            };
        }
    }
}
