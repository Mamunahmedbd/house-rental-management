using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Housing_rental.Models;

namespace Housing_rental.DAL
{
    public class RoleRepository
    {
        public List<Role> GetActiveRoles()
        {
            const string sql = @"
SELECT RoleId, RoleName, Description, IsActive
FROM Roles
WHERE IsActive = 1
ORDER BY RoleName;";

            List<Role> roles = new List<Role>();

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        roles.Add(new Role
                        {
                            RoleId = Convert.ToInt32(reader["RoleId"]),
                            RoleName = Convert.ToString(reader["RoleName"]),
                            Description = Convert.ToString(reader["Description"]),
                            IsActive = Convert.ToBoolean(reader["IsActive"])
                        });
                    }
                }
            }

            return roles;
        }
    }
}
