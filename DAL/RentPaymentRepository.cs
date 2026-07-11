using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Housing_rental.Models;

namespace Housing_rental.DAL
{
    public class RentPaymentRepository
    {
        public List<PaymentAgreementItem> SearchAgreementContexts(string searchText)
        {
            const string sql = @"
SELECT
    a.AgreementId,
    a.AgreementNo,
    a.TenantId,
    t.FullName AS TenantName,
    t.Phone AS TenantPhone,
    p.PropertyName,
    h.HouseName,
    r.RoomNo,
    a.StartDate,
    a.EndDate,
    a.MonthlyRent,
    a.Status AS AgreementStatus,
    ar.TotalDue,
    ar.TotalPaid,
    ar.TotalBalance,
    ar.OverdueAmount
FROM dbo.RentalAgreements a
INNER JOIN dbo.Tenants t ON t.TenantId = a.TenantId
INNER JOIN dbo.Rooms r ON r.RoomId = a.RoomId
INNER JOIN dbo.Houses h ON h.HouseId = r.HouseId
INNER JOIN dbo.Properties p ON p.PropertyId = h.PropertyId
INNER JOIN dbo.vw_AgreementReceivables ar ON ar.AgreementId = a.AgreementId
WHERE
    (a.Status = 'Active' OR ar.TotalBalance > 0)
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
ORDER BY
    CASE WHEN ar.OverdueAmount > 0 THEN 1 WHEN ar.TotalBalance > 0 THEN 2 ELSE 3 END,
    t.FullName,
    a.AgreementNo;";

            List<PaymentAgreementItem> items = new List<PaymentAgreementItem>();

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                AddNVarChar(command, "@SearchText", 150, searchText ?? string.Empty);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(MapAgreement(reader));
                    }
                }
            }

            return items;
        }

        public List<RentChargeListItem> SearchCharges(string searchText, string chargeStatus, int? agreementId, bool includePaid)
        {
            const string sql = @"
SELECT
    b.ChargeId,
    b.AgreementId,
    a.AgreementNo,
    t.FullName AS TenantName,
    p.PropertyName,
    h.HouseName,
    r.RoomNo,
    b.ChargeType,
    b.BillingPeriod,
    b.DueDate,
    b.Amount,
    b.PaidAmount,
    b.BalanceAmount,
    b.CurrencyCode,
    b.ChargeStatus
FROM dbo.vw_RentChargeBalances b
INNER JOIN dbo.RentalAgreements a ON a.AgreementId = b.AgreementId
INNER JOIN dbo.Tenants t ON t.TenantId = a.TenantId
INNER JOIN dbo.Rooms r ON r.RoomId = a.RoomId
INNER JOIN dbo.Houses h ON h.HouseId = r.HouseId
INNER JOIN dbo.Properties p ON p.PropertyId = h.PropertyId
WHERE
    (@AgreementId IS NULL OR b.AgreementId = @AgreementId)
    AND (@IncludePaid = 1 OR b.BalanceAmount > 0)
    AND (@ChargeStatus = '' OR b.ChargeStatus = @ChargeStatus)
    AND
    (
        @SearchText = ''
        OR a.AgreementNo LIKE '%' + @SearchText + '%'
        OR t.FullName LIKE '%' + @SearchText + '%'
        OR p.PropertyName LIKE '%' + @SearchText + '%'
        OR h.HouseName LIKE '%' + @SearchText + '%'
        OR r.RoomNo LIKE '%' + @SearchText + '%'
    )
ORDER BY b.DueDate, b.ChargeId;";

            List<RentChargeListItem> items = new List<RentChargeListItem>();

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                AddNVarChar(command, "@SearchText", 150, searchText ?? string.Empty);
                AddNVarChar(command, "@ChargeStatus", 20, NormalizeStatusFilter(chargeStatus));
                command.Parameters.Add("@AgreementId", SqlDbType.Int).Value = (object)agreementId ?? DBNull.Value;
                command.Parameters.Add("@IncludePaid", SqlDbType.Bit).Value = includePaid;
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(MapCharge(reader));
                    }
                }
            }

            return items;
        }

        public List<PaymentListItem> SearchPayments(string searchText, string status, DateTime? dateFrom, DateTime? dateTo)
        {
            const string sql = @"
SELECT
    PaymentId,
    ReceiptNo,
    TenantId,
    TenantName,
    AgreementId,
    AgreementNo,
    PropertyName,
    HouseName,
    RoomNo,
    PaymentDate,
    PostedAt,
    Amount,
    CurrencyCode,
    PaymentMethod,
    ExternalReference,
    Status,
    CollectedByName,
    ReversalReason,
    ReversedAt,
    ReversedByName
FROM dbo.vw_PaymentHistory
WHERE
    (@Status = '' OR Status = @Status)
    AND (@DateFrom IS NULL OR PaymentDate >= @DateFrom)
    AND (@DateTo IS NULL OR PaymentDate <= @DateTo)
    AND
    (
        @SearchText = ''
        OR ReceiptNo LIKE '%' + @SearchText + '%'
        OR TenantName LIKE '%' + @SearchText + '%'
        OR AgreementNo LIKE '%' + @SearchText + '%'
        OR PropertyName LIKE '%' + @SearchText + '%'
        OR HouseName LIKE '%' + @SearchText + '%'
        OR RoomNo LIKE '%' + @SearchText + '%'
        OR ISNULL(ExternalReference, '') LIKE '%' + @SearchText + '%'
    )
ORDER BY PaymentDate DESC, PaymentId DESC;";

            List<PaymentListItem> items = new List<PaymentListItem>();

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                AddNVarChar(command, "@SearchText", 150, searchText ?? string.Empty);
                AddNVarChar(command, "@Status", 20, status == "All" ? string.Empty : status ?? string.Empty);
                command.Parameters.Add("@DateFrom", SqlDbType.Date).Value = (object)dateFrom ?? DBNull.Value;
                command.Parameters.Add("@DateTo", SqlDbType.Date).Value = (object)dateTo ?? DBNull.Value;
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(MapPayment(reader));
                    }
                }
            }

            return items;
        }

        public PaymentListItem GetPaymentById(long paymentId)
        {
            const string sql = @"
SELECT
    PaymentId, ReceiptNo, TenantId, TenantName, AgreementId, AgreementNo,
    PropertyName, HouseName, RoomNo, PaymentDate, PostedAt, Amount, CurrencyCode,
    PaymentMethod, ExternalReference, Status, CollectedByName,
    ReversalReason, ReversedAt, ReversedByName
FROM dbo.vw_PaymentHistory
WHERE PaymentId = @PaymentId;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@PaymentId", SqlDbType.BigInt).Value = paymentId;
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? MapPayment(reader) : null;
                }
            }
        }

        public List<PaymentAllocationDetail> GetPaymentAllocations(long paymentId)
        {
            const string sql = @"
SELECT ChargeId, BillingPeriod, DueDate, ChargeAmount, AllocatedAmount, CurrencyCode
FROM dbo.vw_PaymentAllocationDetails
WHERE PaymentId = @PaymentId
ORDER BY BillingPeriod, ChargeId;";

            List<PaymentAllocationDetail> items = new List<PaymentAllocationDetail>();

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@PaymentId", SqlDbType.BigInt).Value = paymentId;
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new PaymentAllocationDetail
                        {
                            ChargeId = Convert.ToInt64(reader["ChargeId"]),
                            BillingPeriod = Convert.ToDateTime(reader["BillingPeriod"]),
                            DueDate = Convert.ToDateTime(reader["DueDate"]),
                            ChargeAmount = Convert.ToDecimal(reader["ChargeAmount"]),
                            AllocatedAmount = Convert.ToDecimal(reader["AllocatedAmount"]),
                            CurrencyCode = Convert.ToString(reader["CurrencyCode"])
                        });
                    }
                }
            }

            return items;
        }

        public PostPaymentResult PostPayment(PostPaymentRequest request, int collectedByUserId)
        {
            DataTable allocationTable = new DataTable();
            allocationTable.Columns.Add("ChargeId", typeof(long));
            allocationTable.Columns.Add("Amount", typeof(decimal));

            foreach (PaymentAllocationRequest allocation in request.Allocations)
            {
                allocationTable.Rows.Add(allocation.ChargeId, allocation.Amount);
            }

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand("dbo.sp_Payment_Post", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@RequestId", SqlDbType.UniqueIdentifier).Value = request.RequestId;
                command.Parameters.Add("@TenantId", SqlDbType.Int).Value = request.TenantId;
                command.Parameters.Add("@AgreementId", SqlDbType.Int).Value = request.AgreementId;
                command.Parameters.Add("@PaymentDate", SqlDbType.Date).Value = request.PaymentDate.Date;
                AddDecimal(command, "@Amount", request.Amount);
                AddChar(command, "@CurrencyCode", 3, request.CurrencyCode);
                AddNVarChar(command, "@PaymentMethod", 30, request.PaymentMethod);
                AddNullableNVarChar(command, "@ExternalReference", 100, request.ExternalReference);
                AddNullableNVarChar(command, "@Remarks", 300, request.Remarks);
                command.Parameters.Add("@CollectedByUserId", SqlDbType.Int).Value = collectedByUserId;

                SqlParameter allocations = command.Parameters.Add("@Allocations", SqlDbType.Structured);
                allocations.TypeName = "dbo.PaymentAllocationInput";
                allocations.Value = allocationTable;

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new InvalidOperationException("Payment posting returned no result.");
                    }

                    return new PostPaymentResult
                    {
                        PaymentId = Convert.ToInt64(reader["PaymentId"]),
                        ReceiptNo = Convert.ToString(reader["ReceiptNo"]),
                        Amount = Convert.ToDecimal(reader["Amount"]),
                        CurrencyCode = Convert.ToString(reader["CurrencyCode"]),
                        Status = Convert.ToString(reader["Status"]),
                        AlreadyProcessed = Convert.ToBoolean(reader["AlreadyProcessed"])
                    };
                }
            }
        }

        public PostPaymentResult ReversePayment(ReversePaymentRequest request, int reversedByUserId)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand("dbo.sp_Payment_Reverse", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@PaymentId", SqlDbType.BigInt).Value = request.PaymentId;
                command.Parameters.Add("@RequestId", SqlDbType.UniqueIdentifier).Value = request.RequestId;
                AddNVarChar(command, "@Reason", 500, request.Reason);
                command.Parameters.Add("@ReversedByUserId", SqlDbType.Int).Value = reversedByUserId;
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new InvalidOperationException("Payment reversal returned no result.");
                    }

                    return new PostPaymentResult
                    {
                        PaymentId = Convert.ToInt64(reader["PaymentId"]),
                        ReceiptNo = Convert.ToString(reader["ReceiptNo"]),
                        Status = Convert.ToString(reader["Status"]),
                        AlreadyProcessed = Convert.ToBoolean(reader["AlreadyProcessed"])
                    };
                }
            }
        }

        public ChargeGenerationResult GenerateMonthlyCharges(DateTime billingPeriod, int createdByUserId, Guid generationRunId)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand("dbo.sp_Payment_GenerateMonthlyCharges", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@BillingPeriod", SqlDbType.Date).Value = billingPeriod.Date;
                command.Parameters.Add("@CreatedByUserId", SqlDbType.Int).Value = createdByUserId;
                command.Parameters.Add("@GenerationRunId", SqlDbType.UniqueIdentifier).Value = generationRunId;
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new InvalidOperationException("Charge generation returned no result.");
                    }

                    return new ChargeGenerationResult
                    {
                        CreatedCount = Convert.ToInt32(reader["CreatedCount"]),
                        BillingPeriod = Convert.ToDateTime(reader["BillingPeriod"]),
                        GenerationRunId = (Guid)reader["GenerationRunId"]
                    };
                }
            }
        }

        public string GetSetting(string settingKey, string defaultValue)
        {
            const string sql = "SELECT SettingValue FROM dbo.AppSettings WHERE SettingKey = @SettingKey;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                AddNVarChar(command, "@SettingKey", 100, settingKey);
                connection.Open();
                object value = command.ExecuteScalar();
                return value == null || value == DBNull.Value ? defaultValue : Convert.ToString(value);
            }
        }

        private static PaymentAgreementItem MapAgreement(SqlDataReader reader)
        {
            return new PaymentAgreementItem
            {
                AgreementId = Convert.ToInt32(reader["AgreementId"]),
                AgreementNo = Convert.ToString(reader["AgreementNo"]),
                TenantId = Convert.ToInt32(reader["TenantId"]),
                TenantName = Convert.ToString(reader["TenantName"]),
                TenantPhone = Convert.ToString(reader["TenantPhone"]),
                PropertyName = Convert.ToString(reader["PropertyName"]),
                HouseName = Convert.ToString(reader["HouseName"]),
                RoomNo = Convert.ToString(reader["RoomNo"]),
                StartDate = Convert.ToDateTime(reader["StartDate"]),
                EndDate = Convert.ToDateTime(reader["EndDate"]),
                MonthlyRent = Convert.ToDecimal(reader["MonthlyRent"]),
                AgreementStatus = Convert.ToString(reader["AgreementStatus"]),
                TotalDue = Convert.ToDecimal(reader["TotalDue"]),
                TotalPaid = Convert.ToDecimal(reader["TotalPaid"]),
                TotalBalance = Convert.ToDecimal(reader["TotalBalance"]),
                OverdueAmount = Convert.ToDecimal(reader["OverdueAmount"])
            };
        }

        private static RentChargeListItem MapCharge(SqlDataReader reader)
        {
            return new RentChargeListItem
            {
                ChargeId = Convert.ToInt64(reader["ChargeId"]),
                AgreementId = Convert.ToInt32(reader["AgreementId"]),
                AgreementNo = Convert.ToString(reader["AgreementNo"]),
                TenantName = Convert.ToString(reader["TenantName"]),
                PropertyName = Convert.ToString(reader["PropertyName"]),
                HouseName = Convert.ToString(reader["HouseName"]),
                RoomNo = Convert.ToString(reader["RoomNo"]),
                ChargeType = Convert.ToString(reader["ChargeType"]),
                BillingPeriod = Convert.ToDateTime(reader["BillingPeriod"]),
                DueDate = Convert.ToDateTime(reader["DueDate"]),
                Amount = Convert.ToDecimal(reader["Amount"]),
                PaidAmount = Convert.ToDecimal(reader["PaidAmount"]),
                BalanceAmount = Convert.ToDecimal(reader["BalanceAmount"]),
                CurrencyCode = Convert.ToString(reader["CurrencyCode"]),
                ChargeStatus = Convert.ToString(reader["ChargeStatus"])
            };
        }

        private static PaymentListItem MapPayment(SqlDataReader reader)
        {
            return new PaymentListItem
            {
                PaymentId = Convert.ToInt64(reader["PaymentId"]),
                ReceiptNo = Convert.ToString(reader["ReceiptNo"]),
                TenantId = Convert.ToInt32(reader["TenantId"]),
                TenantName = Convert.ToString(reader["TenantName"]),
                AgreementId = Convert.ToInt32(reader["AgreementId"]),
                AgreementNo = Convert.ToString(reader["AgreementNo"]),
                PropertyName = Convert.ToString(reader["PropertyName"]),
                HouseName = Convert.ToString(reader["HouseName"]),
                RoomNo = Convert.ToString(reader["RoomNo"]),
                PaymentDate = Convert.ToDateTime(reader["PaymentDate"]),
                PostedAt = Convert.ToDateTime(reader["PostedAt"]),
                Amount = Convert.ToDecimal(reader["Amount"]),
                CurrencyCode = Convert.ToString(reader["CurrencyCode"]),
                PaymentMethod = Convert.ToString(reader["PaymentMethod"]),
                ExternalReference = DbString(reader, "ExternalReference"),
                Status = Convert.ToString(reader["Status"]),
                CollectedByName = Convert.ToString(reader["CollectedByName"]),
                ReversalReason = DbString(reader, "ReversalReason"),
                ReversedAt = reader["ReversedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ReversedAt"]),
                ReversedByName = DbString(reader, "ReversedByName")
            };
        }

        private static string DbString(SqlDataReader reader, string columnName)
        {
            return reader[columnName] == DBNull.Value ? null : Convert.ToString(reader[columnName]);
        }

        private static string NormalizeStatusFilter(string value)
        {
            return string.IsNullOrWhiteSpace(value) || value == ChargeStatuses.All ? string.Empty : value.Trim();
        }

        private static void AddNVarChar(SqlCommand command, string name, int size, string value)
        {
            command.Parameters.Add(name, SqlDbType.NVarChar, size).Value = value ?? string.Empty;
        }

        private static void AddNullableNVarChar(SqlCommand command, string name, int size, string value)
        {
            command.Parameters.Add(name, SqlDbType.NVarChar, size).Value =
                string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim();
        }

        private static void AddChar(SqlCommand command, string name, int size, string value)
        {
            command.Parameters.Add(name, SqlDbType.Char, size).Value = value;
        }

        private static void AddDecimal(SqlCommand command, string name, decimal value)
        {
            SqlParameter parameter = command.Parameters.Add(name, SqlDbType.Decimal);
            parameter.Precision = 18;
            parameter.Scale = 2;
            parameter.Value = value;
        }
    }
}
