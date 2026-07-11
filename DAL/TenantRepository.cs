using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Housing_rental.Models;

namespace Housing_rental.DAL
{
    public class TenantRepository
    {
        public List<Tenant> SearchTenants(string searchText, string status, bool includeInactive)
        {
            const string sql = @"
SELECT
    TenantId,
    FullName,
    Phone,
    Email,
    NationalId,
    Address,
    EmergencyContactName,
    EmergencyContactPhone,
    Status,
    CreatedAt
FROM dbo.Tenants
WHERE
    (@IncludeInactive = 1 OR @Status <> '' OR Status = 'Active')
    AND (@Status = '' OR Status = @Status)
    AND
    (
        @SearchText = ''
        OR FullName LIKE '%' + @SearchText + '%'
        OR Phone LIKE '%' + @SearchText + '%'
        OR ISNULL(Email, '') LIKE '%' + @SearchText + '%'
        OR ISNULL(NationalId, '') LIKE '%' + @SearchText + '%'
        OR ISNULL(Address, '') LIKE '%' + @SearchText + '%'
        OR ISNULL(EmergencyContactName, '') LIKE '%' + @SearchText + '%'
        OR ISNULL(EmergencyContactPhone, '') LIKE '%' + @SearchText + '%'
    )
ORDER BY
    CASE Status
        WHEN 'Active' THEN 1
        WHEN 'Inactive' THEN 2
        ELSE 3
    END,
    FullName ASC;";

            List<Tenant> tenants = new List<Tenant>();

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@SearchText", searchText ?? string.Empty);
                command.Parameters.AddWithValue("@Status", status ?? string.Empty);
                command.Parameters.AddWithValue("@IncludeInactive", includeInactive);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tenants.Add(MapTenant(reader));
                    }
                }
            }

            return tenants;
        }

        public List<Tenant> GetActiveTenants()
        {
            return SearchTenants(string.Empty, "Active", true);
        }

        public Tenant GetTenantById(int tenantId)
        {
            const string sql = @"
SELECT
    TenantId,
    FullName,
    Phone,
    Email,
    NationalId,
    Address,
    EmergencyContactName,
    EmergencyContactPhone,
    Status,
    CreatedAt
FROM dbo.Tenants
WHERE TenantId = @TenantId;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@TenantId", tenantId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? MapTenant(reader) : null;
                }
            }
        }

        public bool NationalIdExists(string nationalId, int excludedTenantId)
        {
            if (string.IsNullOrWhiteSpace(nationalId))
            {
                return false;
            }

            const string sql = @"
SELECT COUNT(1)
FROM dbo.Tenants
WHERE
    TenantId <> @ExcludedTenantId
    AND ISNULL(NationalId, '') <> ''
    AND LOWER(NationalId) = LOWER(@NationalId);";

            return Exists(
                sql,
                SqlHelper.Parameter("@NationalId", nationalId),
                SqlHelper.Parameter("@ExcludedTenantId", excludedTenantId));
        }

        public bool PhoneExists(string phone, int excludedTenantId)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return false;
            }

            const string sql = @"
SELECT COUNT(1)
FROM dbo.Tenants
WHERE
    TenantId <> @ExcludedTenantId
    AND Phone = @Phone;";

            return Exists(
                sql,
                SqlHelper.Parameter("@Phone", phone),
                SqlHelper.Parameter("@ExcludedTenantId", excludedTenantId));
        }

        public int CreateTenant(Tenant tenant)
        {
            const string sql = @"
INSERT INTO dbo.Tenants
(
    FullName,
    Phone,
    Email,
    NationalId,
    Address,
    EmergencyContactName,
    EmergencyContactPhone,
    Status,
    CreatedAt
)
OUTPUT INSERTED.TenantId
VALUES
(
    @FullName,
    @Phone,
    @Email,
    @NationalId,
    @Address,
    @EmergencyContactName,
    @EmergencyContactPhone,
    @Status,
    GETDATE()
);";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                AddTenantParameters(command, tenant);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void UpdateTenant(Tenant tenant)
        {
            const string sql = @"
UPDATE dbo.Tenants
SET
    FullName = @FullName,
    Phone = @Phone,
    Email = @Email,
    NationalId = @NationalId,
    Address = @Address,
    EmergencyContactName = @EmergencyContactName,
    EmergencyContactPhone = @EmergencyContactPhone,
    Status = @Status
WHERE TenantId = @TenantId;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@TenantId", tenant.TenantId);
                AddTenantParameters(command, tenant);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void SetTenantStatus(int tenantId, string status)
        {
            const string sql = "UPDATE dbo.Tenants SET Status = @Status WHERE TenantId = @TenantId;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@TenantId", tenantId);
                command.Parameters.AddWithValue("@Status", status);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public bool TenantHasActiveAgreement(int tenantId)
        {
            const string sql = @"
SELECT COUNT(1)
FROM dbo.RentalAgreements
WHERE TenantId = @TenantId
AND Status = 'Active';";

            return Exists(sql, SqlHelper.Parameter("@TenantId", tenantId));
        }

        public DataTable GetTenantAgreementHistory(int tenantId)
        {
            const string sql = @"
SELECT
    a.AgreementNo,
    p.PropertyName,
    h.HouseName,
    r.RoomNo,
    a.StartDate,
    a.EndDate,
    a.MonthlyRent,
    a.SecurityDeposit,
    a.Status,
    a.CreatedAt
FROM dbo.RentalAgreements a
INNER JOIN dbo.Rooms r ON r.RoomId = a.RoomId
INNER JOIN dbo.Houses h ON h.HouseId = r.HouseId
INNER JOIN dbo.Properties p ON p.PropertyId = h.PropertyId
WHERE a.TenantId = @TenantId
ORDER BY a.StartDate DESC, a.AgreementId DESC;";

            return SqlHelper.ExecuteDataTable(sql, SqlHelper.Parameter("@TenantId", tenantId));
        }

        public DataTable GetTenantPaymentHistory(int tenantId)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand("dbo.sp_GetTenantPaymentHistory", connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@TenantId", tenantId);

                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        public DataTable GetTenantCurrentOccupancy(int tenantId)
        {
            const string sql = @"
SELECT
    a.AgreementNo,
    p.PropertyName,
    h.HouseName,
    r.RoomNo,
    r.RoomType,
    a.StartDate,
    a.EndDate,
    a.MonthlyRent,
    a.SecurityDeposit,
    a.Status AS AgreementStatus,
    r.Status AS RoomStatus
FROM dbo.RentalAgreements a
INNER JOIN dbo.Rooms r ON r.RoomId = a.RoomId
INNER JOIN dbo.Houses h ON h.HouseId = r.HouseId
INNER JOIN dbo.Properties p ON p.PropertyId = h.PropertyId
WHERE a.TenantId = @TenantId
AND a.Status = 'Active'
ORDER BY a.StartDate DESC;";

            return SqlHelper.ExecuteDataTable(sql, SqlHelper.Parameter("@TenantId", tenantId));
        }

        public DataTable GetTenantBalanceSummary(int tenantId)
        {
            const string sql = @"
SELECT
    tr.TenantId,
    tr.FullName,
    tr.TotalDue,
    tr.TotalPaid,
    tr.TotalBalance,
    tr.PaymentCount,
    tr.OverdueCount
FROM dbo.vw_TenantReceivables tr
WHERE tr.TenantId = @TenantId;";

            return SqlHelper.ExecuteDataTable(sql, SqlHelper.Parameter("@TenantId", tenantId));
        }

        public DataTable GetTenantDirectory(string searchText, string status)
        {
            const string sql = @"
SELECT
    t.TenantId,
    t.FullName,
    t.Phone,
    t.Email,
    t.NationalId,
    t.Status,
    a.AgreementNo,
    a.StartDate,
    a.EndDate,
    p.PropertyName,
    h.HouseName,
    r.RoomNo,
    tr.TotalDue,
    tr.TotalPaid,
    tr.TotalBalance
FROM dbo.Tenants t
LEFT JOIN dbo.RentalAgreements a ON a.TenantId = t.TenantId AND a.Status = 'Active'
LEFT JOIN dbo.Rooms r ON r.RoomId = a.RoomId
LEFT JOIN dbo.Houses h ON h.HouseId = r.HouseId
LEFT JOIN dbo.Properties p ON p.PropertyId = h.PropertyId
INNER JOIN dbo.vw_TenantReceivables tr ON tr.TenantId = t.TenantId
WHERE
    (@Status = '' OR t.Status = @Status)
    AND
    (
        @SearchText = ''
        OR t.FullName LIKE '%' + @SearchText + '%'
        OR t.Phone LIKE '%' + @SearchText + '%'
        OR ISNULL(t.Email, '') LIKE '%' + @SearchText + '%'
        OR ISNULL(t.NationalId, '') LIKE '%' + @SearchText + '%'
        OR ISNULL(p.PropertyName, '') LIKE '%' + @SearchText + '%'
        OR ISNULL(r.RoomNo, '') LIKE '%' + @SearchText + '%'
    )
GROUP BY
    t.TenantId,
    t.FullName,
    t.Phone,
    t.Email,
    t.NationalId,
    t.Status,
    a.AgreementNo,
    a.StartDate,
    a.EndDate,
    p.PropertyName,
    h.HouseName,
    r.RoomNo,
    tr.TotalDue,
    tr.TotalPaid,
    tr.TotalBalance
ORDER BY
    CASE t.Status
        WHEN 'Active' THEN 1
        WHEN 'Inactive' THEN 2
        ELSE 3
    END,
    t.FullName ASC;";

            return SqlHelper.ExecuteDataTable(
                sql,
                SqlHelper.Parameter("@SearchText", searchText ?? string.Empty),
                SqlHelper.Parameter("@Status", status ?? string.Empty));
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

        private static void AddTenantParameters(SqlCommand command, Tenant tenant)
        {
            command.Parameters.AddWithValue("@FullName", tenant.FullName);
            command.Parameters.AddWithValue("@Phone", tenant.Phone);
            command.Parameters.Add(SqlHelper.Parameter("@Email", tenant.Email));
            command.Parameters.Add(SqlHelper.Parameter("@NationalId", tenant.NationalId));
            command.Parameters.Add(SqlHelper.Parameter("@Address", tenant.Address));
            command.Parameters.Add(SqlHelper.Parameter("@EmergencyContactName", tenant.EmergencyContactName));
            command.Parameters.Add(SqlHelper.Parameter("@EmergencyContactPhone", tenant.EmergencyContactPhone));
            command.Parameters.AddWithValue("@Status", tenant.Status);
        }

        private static Tenant MapTenant(SqlDataReader reader)
        {
            return new Tenant
            {
                TenantId = Convert.ToInt32(reader["TenantId"]),
                FullName = Convert.ToString(reader["FullName"]),
                Phone = Convert.ToString(reader["Phone"]),
                Email = Convert.ToString(reader["Email"]),
                NationalId = Convert.ToString(reader["NationalId"]),
                Address = Convert.ToString(reader["Address"]),
                EmergencyContactName = Convert.ToString(reader["EmergencyContactName"]),
                EmergencyContactPhone = Convert.ToString(reader["EmergencyContactPhone"]),
                Status = Convert.ToString(reader["Status"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            };
        }
    }
}
