using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Housing_rental.Models;

namespace Housing_rental.DAL
{
    public class AgreementRepository
    {
        public DataTable GetAgreementDirectory(string searchText, string status, int? propertyId, int? tenantId, DateTime? fromDate, DateTime? toDate)
        {
            const string sql = @"
SELECT
    a.AgreementId,
    a.AgreementNo,
    a.TenantId,
    t.FullName AS TenantName,
    t.Phone AS TenantPhone,
    t.Status AS TenantStatus,
    p.PropertyId,
    p.PropertyName,
    h.HouseId,
    h.HouseName,
    r.RoomId,
    r.RoomNo,
    r.RoomType,
    r.Status AS RoomStatus,
    a.StartDate,
    a.EndDate,
    a.MonthlyRent,
    a.SecurityDeposit,
    a.Status AS AgreementStatus,
    a.CreatedAt,
    u.FullName AS CreatedByName,
    ar.TotalDue,
    ar.TotalPaid,
    ar.TotalBalance,
    ar.PaymentCount
FROM dbo.RentalAgreements a
INNER JOIN dbo.Tenants t ON t.TenantId = a.TenantId
INNER JOIN dbo.Rooms r ON r.RoomId = a.RoomId
INNER JOIN dbo.Houses h ON h.HouseId = r.HouseId
INNER JOIN dbo.Properties p ON p.PropertyId = h.PropertyId
INNER JOIN dbo.Users u ON u.UserId = a.CreatedByUserId
INNER JOIN dbo.vw_AgreementReceivables ar ON ar.AgreementId = a.AgreementId
WHERE
    (@Status = '' OR a.Status = @Status)
    AND (@PropertyId IS NULL OR p.PropertyId = @PropertyId)
    AND (@TenantId IS NULL OR a.TenantId = @TenantId)
    AND (@FromDate IS NULL OR a.StartDate >= @FromDate)
    AND (@ToDate IS NULL OR a.EndDate <= @ToDate)
    AND
    (
        @SearchText = ''
        OR a.AgreementNo LIKE '%' + @SearchText + '%'
        OR t.FullName LIKE '%' + @SearchText + '%'
        OR t.Phone LIKE '%' + @SearchText + '%'
        OR p.PropertyName LIKE '%' + @SearchText + '%'
        OR h.HouseName LIKE '%' + @SearchText + '%'
        OR r.RoomNo LIKE '%' + @SearchText + '%'
    )
GROUP BY
    a.AgreementId,
    a.AgreementNo,
    a.TenantId,
    t.FullName,
    t.Phone,
    t.Status,
    p.PropertyId,
    p.PropertyName,
    h.HouseId,
    h.HouseName,
    r.RoomId,
    r.RoomNo,
    r.RoomType,
    r.Status,
    a.StartDate,
    a.EndDate,
    a.MonthlyRent,
    a.SecurityDeposit,
    a.Status,
    a.CreatedAt,
    u.FullName,
    ar.TotalDue,
    ar.TotalPaid,
    ar.TotalBalance,
    ar.PaymentCount
ORDER BY
    CASE a.Status
        WHEN 'Active' THEN 1
        WHEN 'Draft' THEN 2
        WHEN 'Expired' THEN 3
        WHEN 'Terminated' THEN 4
        ELSE 5
    END,
    a.StartDate DESC,
    a.AgreementId DESC;";

            return SqlHelper.ExecuteDataTable(
                sql,
                SqlHelper.Parameter("@SearchText", searchText ?? string.Empty),
                SqlHelper.Parameter("@Status", status ?? string.Empty),
                SqlHelper.Parameter("@PropertyId", propertyId),
                SqlHelper.Parameter("@TenantId", tenantId),
                SqlHelper.Parameter("@FromDate", fromDate),
                SqlHelper.Parameter("@ToDate", toDate));
        }

        public List<RentalAgreement> SearchAgreements(string searchText, string status, DateTime? fromDate, DateTime? toDate)
        {
            const string sql = @"
SELECT
    a.AgreementId,
    a.AgreementNo,
    a.TenantId,
    a.RoomId,
    a.StartDate,
    a.EndDate,
    a.MonthlyRent,
    a.SecurityDeposit,
    a.Status,
    a.Notes,
    a.CreatedByUserId,
    a.CreatedAt
FROM dbo.RentalAgreements a
INNER JOIN dbo.Tenants t ON t.TenantId = a.TenantId
INNER JOIN dbo.Rooms r ON r.RoomId = a.RoomId
WHERE
    (@Status = '' OR a.Status = @Status)
    AND (@FromDate IS NULL OR a.StartDate >= @FromDate)
    AND (@ToDate IS NULL OR a.EndDate <= @ToDate)
    AND
    (
        @SearchText = ''
        OR a.AgreementNo LIKE '%' + @SearchText + '%'
        OR t.FullName LIKE '%' + @SearchText + '%'
        OR r.RoomNo LIKE '%' + @SearchText + '%'
    )
ORDER BY a.StartDate DESC, a.AgreementId DESC;";

            List<RentalAgreement> agreements = new List<RentalAgreement>();

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(SqlHelper.Parameter("@SearchText", searchText ?? string.Empty));
                command.Parameters.Add(SqlHelper.Parameter("@Status", status ?? string.Empty));
                command.Parameters.Add(SqlHelper.Parameter("@FromDate", fromDate));
                command.Parameters.Add(SqlHelper.Parameter("@ToDate", toDate));

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        agreements.Add(MapAgreement(reader));
                    }
                }
            }

            return agreements;
        }

        public RentalAgreement GetAgreementById(int agreementId)
        {
            const string sql = @"
SELECT
    AgreementId,
    AgreementNo,
    TenantId,
    RoomId,
    StartDate,
    EndDate,
    MonthlyRent,
    SecurityDeposit,
    Status,
    Notes,
    CreatedByUserId,
    CreatedAt
FROM dbo.RentalAgreements
WHERE AgreementId = @AgreementId;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@AgreementId", agreementId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? MapAgreement(reader) : null;
                }
            }
        }

        public DataTable GetAgreementDetails(int agreementId)
        {
            const string sql = @"
SELECT
    a.AgreementId,
    a.AgreementNo,
    t.FullName AS TenantName,
    t.Phone AS TenantPhone,
    t.Status AS TenantStatus,
    p.PropertyName,
    h.HouseName,
    r.RoomNo,
    r.RoomType,
    r.Status AS RoomStatus,
    a.StartDate,
    a.EndDate,
    a.MonthlyRent,
    a.SecurityDeposit,
    a.Status AS AgreementStatus,
    a.Notes,
    u.FullName AS CreatedByName,
    a.CreatedAt,
    ar.TotalDue,
    ar.TotalPaid,
    ar.TotalBalance,
    ar.PaymentCount
FROM dbo.RentalAgreements a
INNER JOIN dbo.Tenants t ON t.TenantId = a.TenantId
INNER JOIN dbo.Rooms r ON r.RoomId = a.RoomId
INNER JOIN dbo.Houses h ON h.HouseId = r.HouseId
INNER JOIN dbo.Properties p ON p.PropertyId = h.PropertyId
INNER JOIN dbo.Users u ON u.UserId = a.CreatedByUserId
INNER JOIN dbo.vw_AgreementReceivables ar ON ar.AgreementId = a.AgreementId
WHERE a.AgreementId = @AgreementId
GROUP BY
    a.AgreementId,
    a.AgreementNo,
    t.FullName,
    t.Phone,
    t.Status,
    p.PropertyName,
    h.HouseName,
    r.RoomNo,
    r.RoomType,
    r.Status,
    a.StartDate,
    a.EndDate,
    a.MonthlyRent,
    a.SecurityDeposit,
    a.Status,
    a.Notes,
    u.FullName,
    a.CreatedAt,
    ar.TotalDue,
    ar.TotalPaid,
    ar.TotalBalance,
    ar.PaymentCount;";

            return SqlHelper.ExecuteDataTable(sql, SqlHelper.Parameter("@AgreementId", agreementId));
        }

        public DataTable GetAgreementPaymentHistory(int agreementId)
        {
            const string sql = @"
SELECT
    ph.PaymentId,
    ph.ReceiptNo,
    MONTH(ad.BillingPeriod) AS PaymentMonth,
    YEAR(ad.BillingPeriod) AS PaymentYear,
    ad.ChargeAmount AS DueAmount,
    ad.AllocatedAmount AS PaidAmount,
    cb.BalanceAmount,
    ph.PaymentDate,
    ph.PaymentMethod,
    ph.Status
FROM dbo.vw_PaymentHistory ph
INNER JOIN dbo.vw_PaymentAllocationDetails ad ON ad.PaymentId = ph.PaymentId
INNER JOIN dbo.vw_RentChargeBalances cb ON cb.ChargeId = ad.ChargeId
WHERE ph.AgreementId = @AgreementId
ORDER BY ph.PaymentDate DESC, ph.PaymentId DESC, ad.BillingPeriod DESC;";

            return SqlHelper.ExecuteDataTable(sql, SqlHelper.Parameter("@AgreementId", agreementId));
        }

        public DataTable GetAgreementBalanceSummary(int agreementId)
        {
            const string sql = @"
SELECT
    ar.AgreementId,
    ar.AgreementNo,
    ar.TotalDue,
    ar.TotalPaid,
    ar.TotalBalance,
    ar.PaymentCount,
    ar.OverdueCount
FROM dbo.vw_AgreementReceivables ar
WHERE ar.AgreementId = @AgreementId;";

            return SqlHelper.ExecuteDataTable(sql, SqlHelper.Parameter("@AgreementId", agreementId));
        }

        public DataTable GetActiveAgreements()
        {
            const string sql = @"
SELECT
    a.AgreementId,
    a.AgreementNo,
    t.FullName AS TenantName,
    t.Phone AS TenantPhone,
    p.PropertyName,
    h.HouseName,
    r.RoomNo,
    a.StartDate,
    a.EndDate,
    a.MonthlyRent,
    a.SecurityDeposit,
    a.Status
FROM dbo.RentalAgreements a
INNER JOIN dbo.Tenants t ON t.TenantId = a.TenantId
INNER JOIN dbo.Rooms r ON r.RoomId = a.RoomId
INNER JOIN dbo.Houses h ON h.HouseId = r.HouseId
INNER JOIN dbo.Properties p ON p.PropertyId = h.PropertyId
WHERE a.Status = 'Active'
ORDER BY t.FullName, a.AgreementNo;";

            return SqlHelper.ExecuteDataTable(sql);
        }

        public DataTable GetExpiringAgreements(int daysAhead)
        {
            const string sql = @"
SELECT
    a.AgreementId,
    a.AgreementNo,
    t.FullName AS TenantName,
    t.Phone AS TenantPhone,
    p.PropertyName,
    h.HouseName,
    r.RoomNo,
    a.EndDate,
    DATEDIFF(DAY, CONVERT(DATE, GETDATE()), a.EndDate) AS DaysLeft,
    a.MonthlyRent,
    a.Status
FROM dbo.RentalAgreements a
INNER JOIN dbo.Tenants t ON t.TenantId = a.TenantId
INNER JOIN dbo.Rooms r ON r.RoomId = a.RoomId
INNER JOIN dbo.Houses h ON h.HouseId = r.HouseId
INNER JOIN dbo.Properties p ON p.PropertyId = h.PropertyId
WHERE
    a.Status = 'Active'
    AND a.EndDate BETWEEN CONVERT(DATE, GETDATE()) AND DATEADD(DAY, @DaysAhead, CONVERT(DATE, GETDATE()))
ORDER BY a.EndDate ASC, t.FullName ASC;";

            return SqlHelper.ExecuteDataTable(sql, SqlHelper.Parameter("@DaysAhead", daysAhead));
        }

        public bool AgreementNoExists(string agreementNo, int excludedAgreementId)
        {
            const string sql = @"
SELECT COUNT(1)
FROM dbo.RentalAgreements
WHERE
    AgreementId <> @ExcludedAgreementId
    AND LOWER(AgreementNo) = LOWER(@AgreementNo);";

            return Exists(
                sql,
                SqlHelper.Parameter("@AgreementNo", agreementNo),
                SqlHelper.Parameter("@ExcludedAgreementId", excludedAgreementId));
        }

        public bool RoomHasActiveAgreement(int roomId, int excludedAgreementId)
        {
            const string sql = @"
SELECT COUNT(1)
FROM dbo.RentalAgreements
WHERE
    RoomId = @RoomId
    AND Status = 'Active'
    AND AgreementId <> @ExcludedAgreementId;";

            return Exists(
                sql,
                SqlHelper.Parameter("@RoomId", roomId),
                SqlHelper.Parameter("@ExcludedAgreementId", excludedAgreementId));
        }

        public bool TenantHasActiveAgreement(int tenantId, int excludedAgreementId)
        {
            const string sql = @"
SELECT COUNT(1)
FROM dbo.RentalAgreements
WHERE
    TenantId = @TenantId
    AND Status = 'Active'
    AND AgreementId <> @ExcludedAgreementId;";

            return Exists(
                sql,
                SqlHelper.Parameter("@TenantId", tenantId),
                SqlHelper.Parameter("@ExcludedAgreementId", excludedAgreementId));
        }

        public bool TenantIsActive(int tenantId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Tenants WHERE TenantId = @TenantId AND Status = 'Active';";
            return Exists(sql, SqlHelper.Parameter("@TenantId", tenantId));
        }

        public bool RoomIsAvailable(int roomId)
        {
            const string sql = @"
SELECT COUNT(1)
FROM dbo.Rooms r
INNER JOIN dbo.Houses h ON h.HouseId = r.HouseId
INNER JOIN dbo.Properties p ON p.PropertyId = h.PropertyId
WHERE
    r.RoomId = @RoomId
    AND r.Status = 'Available'
    AND h.IsActive = 1
    AND p.IsActive = 1;";

            return Exists(sql, SqlHelper.Parameter("@RoomId", roomId));
        }

        public bool AgreementHasPayments(int agreementId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Payments WHERE AgreementId = @AgreementId;";
            return Exists(sql, SqlHelper.Parameter("@AgreementId", agreementId));
        }

        public int CreateAgreement(RentalAgreement agreement)
        {
            const string sql = @"
INSERT INTO dbo.RentalAgreements
(
    AgreementNo,
    TenantId,
    RoomId,
    StartDate,
    EndDate,
    MonthlyRent,
    SecurityDeposit,
    Status,
    Notes,
    CreatedByUserId,
    CreatedAt
)
OUTPUT INSERTED.AgreementId
VALUES
(
    @AgreementNo,
    @TenantId,
    @RoomId,
    @StartDate,
    @EndDate,
    @MonthlyRent,
    @SecurityDeposit,
    @Status,
    @Notes,
    @CreatedByUserId,
    GETDATE()
);";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                AddAgreementParameters(command, agreement);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void UpdateDraftAgreement(RentalAgreement agreement)
        {
            const string sql = @"
UPDATE dbo.RentalAgreements
SET
    AgreementNo = @AgreementNo,
    TenantId = @TenantId,
    RoomId = @RoomId,
    StartDate = @StartDate,
    EndDate = @EndDate,
    MonthlyRent = @MonthlyRent,
    SecurityDeposit = @SecurityDeposit,
    Notes = @Notes
WHERE AgreementId = @AgreementId
AND Status = 'Draft';";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@AgreementId", agreement.AgreementId);
                AddAgreementParameters(command, agreement);
                connection.Open();

                if (command.ExecuteNonQuery() == 0)
                {
                    throw new InvalidOperationException("Only draft agreements can be edited.");
                }
            }
        }

        public void UpdateAgreementNotes(int agreementId, string notes)
        {
            const string sql = "UPDATE dbo.RentalAgreements SET Notes = @Notes WHERE AgreementId = @AgreementId;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@AgreementId", agreementId);
                command.Parameters.Add(SqlHelper.Parameter("@Notes", notes));
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void ActivateAgreement(int agreementId)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        AgreementState state = GetAgreementState(connection, transaction, agreementId);

                        if (state == null || state.Status != "Draft")
                        {
                            throw new InvalidOperationException("Only draft agreements can be activated.");
                        }

                        if (!TenantIsActive(connection, transaction, state.TenantId))
                        {
                            throw new InvalidOperationException("The selected tenant is not active.");
                        }

                        if (!RoomIsAvailable(connection, transaction, state.RoomId))
                        {
                            throw new InvalidOperationException("The selected room is no longer available.");
                        }

                        if (RoomHasActiveAgreement(connection, transaction, state.RoomId, agreementId))
                        {
                            throw new InvalidOperationException("This room already has an active agreement.");
                        }

                        ExecuteRequired(
                            connection,
                            transaction,
                            "UPDATE dbo.RentalAgreements SET Status = 'Active' WHERE AgreementId = @AgreementId AND Status = 'Draft';",
                            "Unable to activate agreement.",
                            SqlHelper.Parameter("@AgreementId", agreementId));

                        ExecuteRequired(
                            connection,
                            transaction,
                            "UPDATE dbo.Rooms SET Status = 'Occupied' WHERE RoomId = @RoomId AND Status = 'Available';",
                            "Unable to mark room as occupied.",
                            SqlHelper.Parameter("@RoomId", state.RoomId));

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void TerminateAgreement(int agreementId)
        {
            EndAgreement(agreementId, "Terminated");
        }

        public void ExpireAgreement(int agreementId)
        {
            EndAgreement(agreementId, "Expired");
        }

        public void CancelAgreement(int agreementId)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        AgreementState state = GetAgreementState(connection, transaction, agreementId);

                        if (state == null)
                        {
                            throw new InvalidOperationException("Selected agreement was not found.");
                        }

                        ExecuteRequired(
                            connection,
                            transaction,
                            "UPDATE dbo.RentalAgreements SET Status = 'Cancelled' WHERE AgreementId = @AgreementId AND Status IN ('Draft', 'Active');",
                            "Only draft or eligible active agreements can be cancelled.",
                            SqlHelper.Parameter("@AgreementId", agreementId));

                        ReleaseRoomIfNoActiveAgreement(connection, transaction, state.RoomId);
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public int ExpireDueAgreements(DateTime today)
        {
            const string sql = @"
SELECT AgreementId
FROM dbo.RentalAgreements
WHERE Status = 'Active'
AND EndDate < @Today;";

            List<int> agreementIds = new List<int>();

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(SqlHelper.Parameter("@Today", today.Date));
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        agreementIds.Add(Convert.ToInt32(reader["AgreementId"]));
                    }
                }
            }

            foreach (int agreementId in agreementIds)
            {
                ExpireAgreement(agreementId);
            }

            return agreementIds.Count;
        }

        public int RenewAgreement(int sourceAgreementId, RentalAgreement renewal)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        AgreementState source = GetAgreementState(connection, transaction, sourceAgreementId);

                        if (source == null || source.Status != "Active")
                        {
                            throw new InvalidOperationException("Only active agreements can be renewed.");
                        }

                        ExecuteRequired(
                            connection,
                            transaction,
                            "UPDATE dbo.RentalAgreements SET Status = 'Expired' WHERE AgreementId = @AgreementId AND Status = 'Active';",
                            "Unable to close the existing agreement.",
                            SqlHelper.Parameter("@AgreementId", sourceAgreementId));

                        renewal.TenantId = source.TenantId;
                        renewal.RoomId = source.RoomId;
                        renewal.Status = "Active";

                        int newAgreementId = CreateAgreement(connection, transaction, renewal);

                        ExecuteRequired(
                            connection,
                            transaction,
                            "UPDATE dbo.Rooms SET Status = 'Occupied' WHERE RoomId = @RoomId;",
                            "Unable to keep room occupied for renewal.",
                            SqlHelper.Parameter("@RoomId", source.RoomId));

                        transaction.Commit();
                        return newAgreementId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public string GetNextAgreementNo(DateTime startDate)
        {
            string prefix = "AGR-" + startDate.ToString("yyyyMM") + "-";
            const string sql = @"
SELECT ISNULL(MAX(TRY_CONVERT(INT, RIGHT(AgreementNo, 4))), 0) + 1
FROM dbo.RentalAgreements
WHERE AgreementNo LIKE @Prefix + '%';";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Prefix", prefix);
                connection.Open();
                int next = Convert.ToInt32(command.ExecuteScalar());
                return prefix + next.ToString("0000");
            }
        }

        private void EndAgreement(int agreementId, string newStatus)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        AgreementState state = GetAgreementState(connection, transaction, agreementId);

                        if (state == null || state.Status != "Active")
                        {
                            throw new InvalidOperationException("Only active agreements can be ended.");
                        }

                        ExecuteRequired(
                            connection,
                            transaction,
                            "UPDATE dbo.RentalAgreements SET Status = @Status WHERE AgreementId = @AgreementId AND Status = 'Active';",
                            "Unable to update agreement status.",
                            SqlHelper.Parameter("@Status", newStatus),
                            SqlHelper.Parameter("@AgreementId", agreementId));

                        ReleaseRoomIfNoActiveAgreement(connection, transaction, state.RoomId);
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static int CreateAgreement(SqlConnection connection, SqlTransaction transaction, RentalAgreement agreement)
        {
            const string sql = @"
INSERT INTO dbo.RentalAgreements
(
    AgreementNo,
    TenantId,
    RoomId,
    StartDate,
    EndDate,
    MonthlyRent,
    SecurityDeposit,
    Status,
    Notes,
    CreatedByUserId,
    CreatedAt
)
OUTPUT INSERTED.AgreementId
VALUES
(
    @AgreementNo,
    @TenantId,
    @RoomId,
    @StartDate,
    @EndDate,
    @MonthlyRent,
    @SecurityDeposit,
    @Status,
    @Notes,
    @CreatedByUserId,
    GETDATE()
);";

            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                AddAgreementParameters(command, agreement);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private static AgreementState GetAgreementState(SqlConnection connection, SqlTransaction transaction, int agreementId)
        {
            const string sql = @"
SELECT AgreementId, TenantId, RoomId, Status
FROM dbo.RentalAgreements
WHERE AgreementId = @AgreementId;";

            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@AgreementId", agreementId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return new AgreementState
                    {
                        AgreementId = Convert.ToInt32(reader["AgreementId"]),
                        TenantId = Convert.ToInt32(reader["TenantId"]),
                        RoomId = Convert.ToInt32(reader["RoomId"]),
                        Status = Convert.ToString(reader["Status"])
                    };
                }
            }
        }

        private static bool TenantIsActive(SqlConnection connection, SqlTransaction transaction, int tenantId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Tenants WHERE TenantId = @TenantId AND Status = 'Active';";
            return Exists(connection, transaction, sql, SqlHelper.Parameter("@TenantId", tenantId));
        }

        private static bool RoomIsAvailable(SqlConnection connection, SqlTransaction transaction, int roomId)
        {
            const string sql = @"
SELECT COUNT(1)
FROM dbo.Rooms r
INNER JOIN dbo.Houses h ON h.HouseId = r.HouseId
INNER JOIN dbo.Properties p ON p.PropertyId = h.PropertyId
WHERE
    r.RoomId = @RoomId
    AND r.Status = 'Available'
    AND h.IsActive = 1
    AND p.IsActive = 1;";

            return Exists(connection, transaction, sql, SqlHelper.Parameter("@RoomId", roomId));
        }

        private static bool RoomHasActiveAgreement(SqlConnection connection, SqlTransaction transaction, int roomId, int excludedAgreementId)
        {
            const string sql = @"
SELECT COUNT(1)
FROM dbo.RentalAgreements
WHERE
    RoomId = @RoomId
    AND Status = 'Active'
    AND AgreementId <> @ExcludedAgreementId;";

            return Exists(
                connection,
                transaction,
                sql,
                SqlHelper.Parameter("@RoomId", roomId),
                SqlHelper.Parameter("@ExcludedAgreementId", excludedAgreementId));
        }

        private static void ReleaseRoomIfNoActiveAgreement(SqlConnection connection, SqlTransaction transaction, int roomId)
        {
            if (RoomHasActiveAgreement(connection, transaction, roomId, 0))
            {
                return;
            }

            using (SqlCommand command = new SqlCommand("UPDATE dbo.Rooms SET Status = 'Available' WHERE RoomId = @RoomId AND Status = 'Occupied';", connection, transaction))
            {
                command.Parameters.Add(SqlHelper.Parameter("@RoomId", roomId));
                command.ExecuteNonQuery();
            }
        }

        private static void ExecuteRequired(SqlConnection connection, SqlTransaction transaction, string sql, string errorMessage, params SqlParameter[] parameters)
        {
            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                if (command.ExecuteNonQuery() == 0)
                {
                    throw new InvalidOperationException(errorMessage);
                }
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

        private static bool Exists(SqlConnection connection, SqlTransaction transaction, string sql, params SqlParameter[] parameters)
        {
            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private static void AddAgreementParameters(SqlCommand command, RentalAgreement agreement)
        {
            command.Parameters.AddWithValue("@AgreementNo", agreement.AgreementNo);
            command.Parameters.AddWithValue("@TenantId", agreement.TenantId);
            command.Parameters.AddWithValue("@RoomId", agreement.RoomId);
            command.Parameters.AddWithValue("@StartDate", agreement.StartDate.Date);
            command.Parameters.AddWithValue("@EndDate", agreement.EndDate.Date);
            command.Parameters.AddWithValue("@MonthlyRent", agreement.MonthlyRent);
            command.Parameters.AddWithValue("@SecurityDeposit", agreement.SecurityDeposit);
            command.Parameters.AddWithValue("@Status", agreement.Status);
            command.Parameters.Add(SqlHelper.Parameter("@Notes", agreement.Notes));
            command.Parameters.AddWithValue("@CreatedByUserId", agreement.CreatedByUserId);
        }

        private static RentalAgreement MapAgreement(SqlDataReader reader)
        {
            return new RentalAgreement
            {
                AgreementId = Convert.ToInt32(reader["AgreementId"]),
                AgreementNo = Convert.ToString(reader["AgreementNo"]),
                TenantId = Convert.ToInt32(reader["TenantId"]),
                RoomId = Convert.ToInt32(reader["RoomId"]),
                StartDate = Convert.ToDateTime(reader["StartDate"]),
                EndDate = Convert.ToDateTime(reader["EndDate"]),
                MonthlyRent = Convert.ToDecimal(reader["MonthlyRent"]),
                SecurityDeposit = Convert.ToDecimal(reader["SecurityDeposit"]),
                Status = Convert.ToString(reader["Status"]),
                Notes = Convert.ToString(reader["Notes"]),
                CreatedByUserId = Convert.ToInt32(reader["CreatedByUserId"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            };
        }

        private class AgreementState
        {
            public int AgreementId { get; set; }
            public int TenantId { get; set; }
            public int RoomId { get; set; }
            public string Status { get; set; }
        }
    }
}
