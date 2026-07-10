using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Housing_rental.Models;

namespace Housing_rental.DAL
{
    public class UserRepository
    {
        public List<User> Search(string searchText)
        {
            const string sql = @"
SELECT
    u.UserId,
    u.RoleId,
    r.RoleName,
    u.FullName,
    u.Username,
    u.PasswordHash,
    u.PasswordSalt,
    u.Phone,
    u.Email,
    u.IsActive,
    u.LastLoginAt,
    u.CreatedAt
FROM Users u
INNER JOIN Roles r ON r.RoleId = u.RoleId
WHERE
    @SearchText = ''
    OR u.FullName LIKE '%' + @SearchText + '%'
    OR u.Username LIKE '%' + @SearchText + '%'
    OR r.RoleName LIKE '%' + @SearchText + '%'
    OR ISNULL(u.Phone, '') LIKE '%' + @SearchText + '%'
    OR ISNULL(u.Email, '') LIKE '%' + @SearchText + '%'
ORDER BY u.IsActive DESC, u.FullName ASC;";

            List<User> users = new List<User>();

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@SearchText", searchText ?? string.Empty);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(MapUser(reader));
                    }
                }
            }

            return users;
        }

        public User GetByUsername(string username)
        {
            const string sql = @"
SELECT TOP 1
    u.UserId,
    u.RoleId,
    r.RoleName,
    u.FullName,
    u.Username,
    u.PasswordHash,
    u.PasswordSalt,
    u.Phone,
    u.Email,
    u.IsActive,
    u.LastLoginAt,
    u.CreatedAt
FROM Users u
INNER JOIN Roles r ON r.RoleId = u.RoleId
WHERE u.Username = @Username;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return MapUser(reader);
                }
            }
        }

        public bool UsernameExists(string username, int excludedUserId)
        {
            const string sql = @"
SELECT COUNT(1)
FROM Users
WHERE Username = @Username
AND UserId <> @ExcludedUserId;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@ExcludedUserId", excludedUserId);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public int Create(User user)
        {
            const string sql = @"
INSERT INTO Users
(
    RoleId,
    FullName,
    Username,
    PasswordHash,
    PasswordSalt,
    Phone,
    Email,
    IsActive,
    CreatedAt
)
OUTPUT INSERTED.UserId
VALUES
(
    @RoleId,
    @FullName,
    @Username,
    @PasswordHash,
    @PasswordSalt,
    @Phone,
    @Email,
    @IsActive,
    GETDATE()
);";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                AddUserParameters(command, user);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void Update(User user)
        {
            const string sql = @"
UPDATE Users
SET
    RoleId = @RoleId,
    FullName = @FullName,
    Username = @Username,
    Phone = @Phone,
    Email = @Email,
    IsActive = @IsActive
WHERE UserId = @UserId;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@UserId", user.UserId);
                command.Parameters.AddWithValue("@RoleId", user.RoleId);
                command.Parameters.AddWithValue("@FullName", user.FullName);
                command.Parameters.AddWithValue("@Username", user.Username);
                command.Parameters.Add(SqlHelper.Parameter("@Phone", user.Phone));
                command.Parameters.Add(SqlHelper.Parameter("@Email", user.Email));
                command.Parameters.AddWithValue("@IsActive", user.IsActive);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void SetActiveStatus(int userId, bool isActive)
        {
            const string sql = "UPDATE Users SET IsActive = @IsActive WHERE UserId = @UserId;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@IsActive", isActive);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void ResetPassword(int userId, string passwordHash, string passwordSalt)
        {
            const string sql = @"
UPDATE Users
SET PasswordHash = @PasswordHash,
    PasswordSalt = @PasswordSalt
WHERE UserId = @UserId;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                command.Parameters.Add(SqlHelper.Parameter("@PasswordSalt", passwordSalt));
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateLastLogin(int userId)
        {
            const string sql = "UPDATE Users SET LastLoginAt = GETDATE() WHERE UserId = @UserId;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static void AddUserParameters(SqlCommand command, User user)
        {
            command.Parameters.AddWithValue("@RoleId", user.RoleId);
            command.Parameters.AddWithValue("@FullName", user.FullName);
            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.Add(SqlHelper.Parameter("@PasswordSalt", user.PasswordSalt));
            command.Parameters.Add(SqlHelper.Parameter("@Phone", user.Phone));
            command.Parameters.Add(SqlHelper.Parameter("@Email", user.Email));
            command.Parameters.AddWithValue("@IsActive", user.IsActive);
        }

        private static User MapUser(SqlDataReader reader)
        {
            return new User
            {
                UserId = Convert.ToInt32(reader["UserId"]),
                RoleId = Convert.ToInt32(reader["RoleId"]),
                RoleName = Convert.ToString(reader["RoleName"]),
                FullName = Convert.ToString(reader["FullName"]),
                Username = Convert.ToString(reader["Username"]),
                PasswordHash = Convert.ToString(reader["PasswordHash"]),
                PasswordSalt = Convert.ToString(reader["PasswordSalt"]),
                Phone = Convert.ToString(reader["Phone"]),
                Email = Convert.ToString(reader["Email"]),
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                LastLoginAt = reader["LastLoginAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["LastLoginAt"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            };
        }
    }
}
