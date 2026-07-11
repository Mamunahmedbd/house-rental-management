USE HouseRentalDB;
GO

SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.SchemaVersions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SchemaVersions
    (
        VersionId NVARCHAR(100) NOT NULL CONSTRAINT PK_SchemaVersions PRIMARY KEY,
        Description NVARCHAR(300) NOT NULL,
        AppliedAt DATETIME2(0) NOT NULL CONSTRAINT DF_SchemaVersions_AppliedAt DEFAULT (SYSUTCDATETIME())
    );
END;
GO

IF EXISTS (SELECT 1 FROM dbo.SchemaVersions WHERE VersionId = '20260711_001_PaymentsLedger')
BEGIN
    PRINT 'Migration 20260711_001_PaymentsLedger schema already exists; refreshing programmable objects.';
END;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID('dbo.RentPayments', 'U') IS NOT NULL
    BEGIN
        IF EXISTS
        (
            SELECT AgreementId, PaymentYear, PaymentMonth
            FROM dbo.RentPayments
            GROUP BY AgreementId, PaymentYear, PaymentMonth
            HAVING COUNT(*) > 1
        )
            THROW 51001, 'Legacy RentPayments contains duplicate agreement billing periods. Resolve them before migration.', 1;

        IF EXISTS
        (
            SELECT 1
            FROM dbo.RentPayments
            WHERE PaymentYear NOT BETWEEN 2000 AND 9999
               OR PaidAmount > DueAmount
               OR BalanceAmount <> DueAmount - PaidAmount
               OR (Status = 'Cancelled' AND PaidAmount > 0)
        )
            THROW 51002, 'Legacy RentPayments contains invalid amount, year, balance, or cancelled-payment data.', 1;
    END;

    IF OBJECT_ID('dbo.RentCharges', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.RentCharges
        (
            ChargeId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RentCharges PRIMARY KEY,
            AgreementId INT NOT NULL,
            ChargeType NVARCHAR(30) NOT NULL CONSTRAINT DF_RentCharges_ChargeType DEFAULT ('MonthlyRent'),
            BillingPeriod DATE NOT NULL,
            PeriodStart DATE NOT NULL,
            PeriodEnd DATE NOT NULL,
            DueDate DATE NOT NULL,
            Amount DECIMAL(18,2) NOT NULL,
            CurrencyCode CHAR(3) NOT NULL,
            Description NVARCHAR(250) NULL,
            SourceType NVARCHAR(30) NOT NULL CONSTRAINT DF_RentCharges_SourceType DEFAULT ('AgreementRent'),
            GenerationRunId UNIQUEIDENTIFIER NULL,
            Status NVARCHAR(20) NOT NULL CONSTRAINT DF_RentCharges_Status DEFAULT ('Open'),
            CreatedByUserId INT NOT NULL,
            CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_RentCharges_CreatedAt DEFAULT (SYSUTCDATETIME()),
            RowVersion ROWVERSION NOT NULL,
            CONSTRAINT FK_RentCharges_Agreements FOREIGN KEY (AgreementId) REFERENCES dbo.RentalAgreements(AgreementId),
            CONSTRAINT FK_RentCharges_Users FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users(UserId),
            CONSTRAINT CK_RentCharges_Amount CHECK (Amount > 0),
            CONSTRAINT CK_RentCharges_Period CHECK (PeriodEnd >= PeriodStart),
            CONSTRAINT CK_RentCharges_BillingPeriod CHECK (DAY(BillingPeriod) = 1),
            CONSTRAINT CK_RentCharges_Currency CHECK (CurrencyCode LIKE '[A-Z][A-Z][A-Z]'),
            CONSTRAINT CK_RentCharges_Status CHECK (Status IN ('Open', 'Waived')),
            CONSTRAINT CK_RentCharges_ChargeType CHECK (ChargeType IN ('MonthlyRent', 'Adjustment', 'LateFee'))
        );

        CREATE UNIQUE INDEX UX_RentCharges_MonthlyAgreementPeriod
            ON dbo.RentCharges(AgreementId, ChargeType, BillingPeriod)
            WHERE ChargeType = 'MonthlyRent';

        CREATE INDEX IX_RentCharges_AgreementPeriod
            ON dbo.RentCharges(AgreementId, BillingPeriod)
            INCLUDE (DueDate, Amount, CurrencyCode, Status);

        CREATE INDEX IX_RentCharges_DueDateStatus
            ON dbo.RentCharges(DueDate, Status)
            INCLUDE (AgreementId, BillingPeriod, Amount, CurrencyCode);
    END;

    IF OBJECT_ID('dbo.Payments', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Payments
        (
            PaymentId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Payments PRIMARY KEY,
            ReceiptNo NVARCHAR(50) NOT NULL CONSTRAINT UQ_Payments_ReceiptNo UNIQUE,
            RequestId UNIQUEIDENTIFIER NOT NULL CONSTRAINT UQ_Payments_RequestId UNIQUE,
            TenantId INT NOT NULL,
            AgreementId INT NOT NULL,
            PaymentDate DATE NOT NULL,
            Amount DECIMAL(18,2) NOT NULL,
            CurrencyCode CHAR(3) NOT NULL,
            PaymentMethod NVARCHAR(30) NOT NULL,
            ExternalReference NVARCHAR(100) NULL,
            Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Payments_Status DEFAULT ('Posted'),
            Remarks NVARCHAR(300) NULL,
            CollectedByUserId INT NOT NULL,
            PostedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Payments_PostedAt DEFAULT (SYSUTCDATETIME()),
            RowVersion ROWVERSION NOT NULL,
            CONSTRAINT FK_Payments_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
            CONSTRAINT FK_Payments_Agreements FOREIGN KEY (AgreementId) REFERENCES dbo.RentalAgreements(AgreementId),
            CONSTRAINT FK_Payments_Users FOREIGN KEY (CollectedByUserId) REFERENCES dbo.Users(UserId),
            CONSTRAINT CK_Payments_Amount CHECK (Amount > 0),
            CONSTRAINT CK_Payments_Currency CHECK (CurrencyCode LIKE '[A-Z][A-Z][A-Z]'),
            CONSTRAINT CK_Payments_Status CHECK (Status IN ('Posted', 'Reversed')),
            CONSTRAINT CK_Payments_Method CHECK (PaymentMethod IN ('Cash', 'BankTransfer', 'Card', 'MobileBanking', 'Cheque'))
        );

        CREATE INDEX IX_Payments_AgreementDate
            ON dbo.Payments(AgreementId, PaymentDate DESC)
            INCLUDE (ReceiptNo, Amount, CurrencyCode, Status, PaymentMethod);

        CREATE INDEX IX_Payments_TenantDate
            ON dbo.Payments(TenantId, PaymentDate DESC)
            INCLUDE (AgreementId, ReceiptNo, Amount, CurrencyCode, Status);

        CREATE INDEX IX_Payments_StatusDate
            ON dbo.Payments(Status, PaymentDate)
            INCLUDE (Amount, CurrencyCode, CollectedByUserId);
    END;

    IF OBJECT_ID('dbo.PaymentAllocations', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.PaymentAllocations
        (
            AllocationId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PaymentAllocations PRIMARY KEY,
            PaymentId BIGINT NOT NULL,
            ChargeId BIGINT NOT NULL,
            Amount DECIMAL(18,2) NOT NULL,
            CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_PaymentAllocations_CreatedAt DEFAULT (SYSUTCDATETIME()),
            CONSTRAINT FK_PaymentAllocations_Payments FOREIGN KEY (PaymentId) REFERENCES dbo.Payments(PaymentId),
            CONSTRAINT FK_PaymentAllocations_Charges FOREIGN KEY (ChargeId) REFERENCES dbo.RentCharges(ChargeId),
            CONSTRAINT UQ_PaymentAllocations_PaymentCharge UNIQUE (PaymentId, ChargeId),
            CONSTRAINT CK_PaymentAllocations_Amount CHECK (Amount > 0)
        );

        CREATE INDEX IX_PaymentAllocations_Charge
            ON dbo.PaymentAllocations(ChargeId)
            INCLUDE (PaymentId, Amount);
    END;

    IF OBJECT_ID('dbo.PaymentReversals', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.PaymentReversals
        (
            ReversalId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PaymentReversals PRIMARY KEY,
            PaymentId BIGINT NOT NULL CONSTRAINT UQ_PaymentReversals_Payment UNIQUE,
            RequestId UNIQUEIDENTIFIER NOT NULL CONSTRAINT UQ_PaymentReversals_Request UNIQUE,
            Reason NVARCHAR(500) NOT NULL,
            ReversedByUserId INT NOT NULL,
            ReversedAt DATETIME2(0) NOT NULL CONSTRAINT DF_PaymentReversals_ReversedAt DEFAULT (SYSUTCDATETIME()),
            CONSTRAINT FK_PaymentReversals_Payments FOREIGN KEY (PaymentId) REFERENCES dbo.Payments(PaymentId),
            CONSTRAINT FK_PaymentReversals_Users FOREIGN KEY (ReversedByUserId) REFERENCES dbo.Users(UserId),
            CONSTRAINT CK_PaymentReversals_Reason CHECK (LEN(LTRIM(RTRIM(Reason))) > 0)
        );
    END;

    IF OBJECT_ID('dbo.ReceiptNumberSequence', 'SO') IS NULL
        EXEC('CREATE SEQUENCE dbo.ReceiptNumberSequence AS BIGINT START WITH 1 INCREMENT BY 1 CACHE 20;');

    IF TYPE_ID('dbo.PaymentAllocationInput') IS NULL
        EXEC('CREATE TYPE dbo.PaymentAllocationInput AS TABLE (ChargeId BIGINT NOT NULL PRIMARY KEY, Amount DECIMAL(18,2) NOT NULL);');

    IF OBJECT_ID('dbo.RentPayments', 'U') IS NOT NULL
    BEGIN
        DECLARE @DefaultCurrency CHAR(3) = UPPER(LEFT(ISNULL((SELECT SettingValue FROM dbo.AppSettings WHERE SettingKey = 'DefaultCurrency'), 'USD'), 3));

        INSERT INTO dbo.RentCharges
        (
            AgreementId, ChargeType, BillingPeriod, PeriodStart, PeriodEnd, DueDate,
            Amount, CurrencyCode, Description, SourceType, Status, CreatedByUserId, CreatedAt
        )
        SELECT
            rp.AgreementId,
            'MonthlyRent',
            DATEFROMPARTS(rp.PaymentYear, rp.PaymentMonth, 1),
            DATEFROMPARTS(rp.PaymentYear, rp.PaymentMonth, 1),
            EOMONTH(DATEFROMPARTS(rp.PaymentYear, rp.PaymentMonth, 1)),
            EOMONTH(DATEFROMPARTS(rp.PaymentYear, rp.PaymentMonth, 1)),
            rp.DueAmount,
            @DefaultCurrency,
            CONCAT('Migrated rent for ', DATENAME(MONTH, DATEFROMPARTS(rp.PaymentYear, rp.PaymentMonth, 1)), ' ', rp.PaymentYear),
            'AgreementRent',
            CASE WHEN rp.Status = 'Cancelled' THEN 'Waived' ELSE 'Open' END,
            rp.CollectedByUserId,
            CONVERT(DATETIME2(0), rp.CreatedAt)
        FROM dbo.RentPayments rp;

        INSERT INTO dbo.Payments
        (
            ReceiptNo, RequestId, TenantId, AgreementId, PaymentDate, Amount, CurrencyCode,
            PaymentMethod, ExternalReference, Status, Remarks, CollectedByUserId, PostedAt
        )
        SELECT
            rp.ReceiptNo,
            NEWID(),
            a.TenantId,
            rp.AgreementId,
            rp.PaymentDate,
            rp.PaidAmount,
            @DefaultCurrency,
            CASE
                WHEN rp.PaymentMethod IN ('Cash', 'BankTransfer', 'Card', 'MobileBanking', 'Cheque') THEN rp.PaymentMethod
                ELSE 'Cash'
            END,
            NULL,
            'Posted',
            rp.Remarks,
            rp.CollectedByUserId,
            CONVERT(DATETIME2(0), rp.CreatedAt)
        FROM dbo.RentPayments rp
        INNER JOIN dbo.RentalAgreements a ON a.AgreementId = rp.AgreementId
        WHERE rp.PaidAmount > 0 AND rp.Status <> 'Cancelled';

        INSERT INTO dbo.PaymentAllocations (PaymentId, ChargeId, Amount, CreatedAt)
        SELECT
            p.PaymentId,
            c.ChargeId,
            rp.PaidAmount,
            CONVERT(DATETIME2(0), rp.CreatedAt)
        FROM dbo.RentPayments rp
        INNER JOIN dbo.Payments p ON p.ReceiptNo = rp.ReceiptNo
        INNER JOIN dbo.RentCharges c
            ON c.AgreementId = rp.AgreementId
           AND c.ChargeType = 'MonthlyRent'
           AND c.BillingPeriod = DATEFROMPARTS(rp.PaymentYear, rp.PaymentMonth, 1)
        WHERE rp.PaidAmount > 0 AND rp.Status <> 'Cancelled';

        EXEC sp_rename 'dbo.RentPayments', 'LegacyRentPayments';
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersions WHERE VersionId = '20260711_001_PaymentsLedger')
    BEGIN
        INSERT INTO dbo.SchemaVersions (VersionId, Description)
        VALUES ('20260711_001_PaymentsLedger', 'Create rent charge, payment, allocation, reversal, sequence, and migration foundation.');
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

CREATE OR ALTER VIEW dbo.vw_RentChargeBalances
AS
SELECT
    c.ChargeId,
    c.AgreementId,
    c.ChargeType,
    c.BillingPeriod,
    c.PeriodStart,
    c.PeriodEnd,
    c.DueDate,
    c.Amount,
    c.CurrencyCode,
    c.Description,
    c.Status AS ChargeRecordStatus,
    ISNULL(x.AllocatedAmount, 0) AS PaidAmount,
    c.Amount - ISNULL(x.AllocatedAmount, 0) AS BalanceAmount,
    CASE
        WHEN c.Status = 'Waived' THEN 'Waived'
        WHEN c.Amount - ISNULL(x.AllocatedAmount, 0) = 0 THEN 'Paid'
        WHEN c.Amount - ISNULL(x.AllocatedAmount, 0) > 0 AND c.DueDate < CONVERT(DATE, GETDATE()) THEN 'Overdue'
        WHEN ISNULL(x.AllocatedAmount, 0) > 0 THEN 'Partial'
        ELSE 'Due'
    END AS ChargeStatus,
    c.CreatedByUserId,
    c.CreatedAt
FROM dbo.RentCharges c
OUTER APPLY
(
    SELECT SUM(pa.Amount) AS AllocatedAmount
    FROM dbo.PaymentAllocations pa
    INNER JOIN dbo.Payments p ON p.PaymentId = pa.PaymentId AND p.Status = 'Posted'
    WHERE pa.ChargeId = c.ChargeId
) x;
GO

CREATE OR ALTER VIEW dbo.vw_PaymentHistory
AS
SELECT
    pmt.PaymentId,
    pmt.ReceiptNo,
    pmt.RequestId,
    pmt.TenantId,
    t.FullName AS TenantName,
    t.Phone AS TenantPhone,
    pmt.AgreementId,
    a.AgreementNo,
    pr.PropertyId,
    pr.PropertyName,
    h.HouseId,
    h.HouseName,
    r.RoomId,
    r.RoomNo,
    pmt.PaymentDate,
    pmt.PostedAt,
    pmt.Amount,
    pmt.CurrencyCode,
    pmt.PaymentMethod,
    pmt.ExternalReference,
    pmt.Status,
    pmt.Remarks,
    pmt.CollectedByUserId,
    u.FullName AS CollectedByName,
    rev.ReversalId,
    rev.Reason AS ReversalReason,
    rev.ReversedAt,
    ru.FullName AS ReversedByName
FROM dbo.Payments pmt
INNER JOIN dbo.Tenants t ON t.TenantId = pmt.TenantId
INNER JOIN dbo.RentalAgreements a ON a.AgreementId = pmt.AgreementId
INNER JOIN dbo.Rooms r ON r.RoomId = a.RoomId
INNER JOIN dbo.Houses h ON h.HouseId = r.HouseId
INNER JOIN dbo.Properties pr ON pr.PropertyId = h.PropertyId
INNER JOIN dbo.Users u ON u.UserId = pmt.CollectedByUserId
LEFT JOIN dbo.PaymentReversals rev ON rev.PaymentId = pmt.PaymentId
LEFT JOIN dbo.Users ru ON ru.UserId = rev.ReversedByUserId;
GO

CREATE OR ALTER VIEW dbo.vw_PaymentAllocationDetails
AS
SELECT
    pa.AllocationId,
    pa.PaymentId,
    p.ReceiptNo,
    pa.ChargeId,
    c.AgreementId,
    c.ChargeType,
    c.BillingPeriod,
    c.DueDate,
    c.Amount AS ChargeAmount,
    pa.Amount AS AllocatedAmount,
    c.CurrencyCode,
    p.Status AS PaymentStatus,
    pa.CreatedAt
FROM dbo.PaymentAllocations pa
INNER JOIN dbo.Payments p ON p.PaymentId = pa.PaymentId
INNER JOIN dbo.RentCharges c ON c.ChargeId = pa.ChargeId;
GO

CREATE OR ALTER VIEW dbo.vw_AgreementReceivables
AS
SELECT
    a.AgreementId,
    a.AgreementNo,
    ISNULL(SUM(CASE WHEN b.ChargeRecordStatus <> 'Waived' THEN b.Amount ELSE 0 END), 0) AS TotalDue,
    ISNULL(SUM(CASE WHEN b.ChargeRecordStatus <> 'Waived' THEN b.PaidAmount ELSE 0 END), 0) AS TotalPaid,
    ISNULL(SUM(CASE WHEN b.ChargeRecordStatus <> 'Waived' THEN b.BalanceAmount ELSE 0 END), 0) AS TotalBalance,
    ISNULL(SUM(CASE WHEN b.ChargeStatus = 'Overdue' THEN b.BalanceAmount ELSE 0 END), 0) AS OverdueAmount,
    SUM(CASE WHEN b.ChargeStatus = 'Overdue' THEN 1 ELSE 0 END) AS OverdueCount,
    COUNT(b.ChargeId) AS ChargeCount,
    (SELECT COUNT(*) FROM dbo.Payments p WHERE p.AgreementId = a.AgreementId) AS PaymentCount
FROM dbo.RentalAgreements a
LEFT JOIN dbo.vw_RentChargeBalances b ON b.AgreementId = a.AgreementId
GROUP BY a.AgreementId, a.AgreementNo;
GO

CREATE OR ALTER VIEW dbo.vw_TenantReceivables
AS
SELECT
    t.TenantId,
    t.FullName,
    t.Phone,
    ISNULL(SUM(ar.TotalDue), 0) AS TotalDue,
    ISNULL(SUM(ar.TotalPaid), 0) AS TotalPaid,
    ISNULL(SUM(ar.TotalBalance), 0) AS TotalBalance,
    ISNULL(SUM(ar.OverdueAmount), 0) AS OverdueAmount,
    ISNULL(SUM(ar.OverdueCount), 0) AS OverdueCount,
    ISNULL(SUM(ar.ChargeCount), 0) AS ChargeCount,
    ISNULL(SUM(ar.PaymentCount), 0) AS PaymentCount
FROM dbo.Tenants t
LEFT JOIN dbo.RentalAgreements a ON a.TenantId = t.TenantId
LEFT JOIN dbo.vw_AgreementReceivables ar ON ar.AgreementId = a.AgreementId
GROUP BY t.TenantId, t.FullName, t.Phone;
GO

CREATE OR ALTER VIEW dbo.vw_AgreementDirectory
AS
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
    a.CreatedByUserId,
    u.FullName AS CreatedByName,
    a.CreatedAt,
    ar.TotalDue,
    ar.TotalPaid,
    ar.TotalBalance,
    ar.OverdueAmount,
    ar.OverdueCount,
    ar.PaymentCount
FROM dbo.RentalAgreements a
INNER JOIN dbo.Tenants t ON t.TenantId = a.TenantId
INNER JOIN dbo.Rooms r ON r.RoomId = a.RoomId
INNER JOIN dbo.Houses h ON h.HouseId = r.HouseId
INNER JOIN dbo.Properties p ON p.PropertyId = h.PropertyId
INNER JOIN dbo.Users u ON u.UserId = a.CreatedByUserId
INNER JOIN dbo.vw_AgreementReceivables ar ON ar.AgreementId = a.AgreementId;
GO

CREATE OR ALTER VIEW dbo.vw_RentCollectionSummary
AS
SELECT
    YEAR(BillingPeriod) AS PaymentYear,
    MONTH(BillingPeriod) AS PaymentMonth,
    SUM(CASE WHEN ChargeRecordStatus <> 'Waived' THEN Amount ELSE 0 END) AS TotalDue,
    SUM(CASE WHEN ChargeRecordStatus <> 'Waived' THEN PaidAmount ELSE 0 END) AS TotalPaid,
    SUM(CASE WHEN ChargeRecordStatus <> 'Waived' THEN BalanceAmount ELSE 0 END) AS TotalBalance,
    COUNT(CASE WHEN ChargeRecordStatus <> 'Waived' THEN 1 END) AS PaymentCount
FROM dbo.vw_RentChargeBalances
GROUP BY YEAR(BillingPeriod), MONTH(BillingPeriod);
GO

CREATE OR ALTER VIEW dbo.vw_TenantBalances
AS
SELECT
    TenantId,
    FullName,
    Phone,
    TotalDue,
    TotalPaid,
    TotalBalance,
    PaymentCount,
    OverdueCount
FROM dbo.vw_TenantReceivables;
GO

CREATE OR ALTER VIEW dbo.vw_MonthlyRentCollectionSummary
AS
SELECT
    YEAR(c.BillingPeriod) AS PaymentYear,
    MONTH(c.BillingPeriod) AS PaymentMonth,
    SUM(CASE WHEN c.ChargeRecordStatus <> 'Waived' THEN c.Amount ELSE 0 END) AS TotalDue,
    SUM(CASE WHEN c.ChargeRecordStatus <> 'Waived' THEN c.PaidAmount ELSE 0 END) AS TotalPaid,
    SUM(CASE WHEN c.ChargeRecordStatus <> 'Waived' THEN c.BalanceAmount ELSE 0 END) AS TotalBalance,
    COUNT(CASE WHEN c.ChargeRecordStatus <> 'Waived' THEN 1 END) AS ChargeCount,
    SUM(CASE WHEN c.ChargeStatus = 'Overdue' THEN 1 ELSE 0 END) AS OverdueCount
FROM dbo.vw_RentChargeBalances c
GROUP BY YEAR(c.BillingPeriod), MONTH(c.BillingPeriod);
GO

CREATE OR ALTER PROCEDURE dbo.sp_Payment_GenerateMonthlyCharges
    @BillingPeriod DATE,
    @CreatedByUserId INT,
    @GenerationRunId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET ANSI_WARNINGS ON;
    SET ARITHABORT ON;
    SET CONCAT_NULL_YIELDS_NULL ON;
    SET NUMERIC_ROUNDABORT OFF;

    DECLARE @PeriodStart DATE = DATEFROMPARTS(YEAR(@BillingPeriod), MONTH(@BillingPeriod), 1);
    DECLARE @PeriodEnd DATE = EOMONTH(@PeriodStart);
    DECLARE @DueDay INT = TRY_CONVERT(INT, (SELECT SettingValue FROM dbo.AppSettings WHERE SettingKey = 'RentDueDay'));
    DECLARE @Currency CHAR(3) = UPPER(LEFT(ISNULL((SELECT SettingValue FROM dbo.AppSettings WHERE SettingKey = 'DefaultCurrency'), 'USD'), 3));
    DECLARE @CreatedCount INT = 0;

    IF @DueDay IS NULL OR @DueDay NOT BETWEEN 1 AND 31 SET @DueDay = 5;
    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE UserId = @CreatedByUserId AND IsActive = 1)
        THROW 51101, 'The charge-generation user is invalid or inactive.', 1;

    BEGIN TRANSACTION;

    INSERT INTO dbo.RentCharges
    (
        AgreementId, ChargeType, BillingPeriod, PeriodStart, PeriodEnd, DueDate,
        Amount, CurrencyCode, Description, SourceType, GenerationRunId, Status, CreatedByUserId
    )
    SELECT
        a.AgreementId,
        'MonthlyRent',
        @PeriodStart,
        @PeriodStart,
        @PeriodEnd,
        CASE
            WHEN a.StartDate > due.StandardDueDate THEN a.StartDate
            WHEN a.EndDate < due.StandardDueDate THEN a.EndDate
            ELSE due.StandardDueDate
        END,
        a.MonthlyRent,
        @Currency,
        CONCAT('Monthly rent for ', DATENAME(MONTH, @PeriodStart), ' ', YEAR(@PeriodStart)),
        'AgreementRent',
        @GenerationRunId,
        'Open',
        @CreatedByUserId
    FROM dbo.RentalAgreements a WITH (UPDLOCK, HOLDLOCK)
    CROSS APPLY
    (
        SELECT DATEFROMPARTS(
            YEAR(@PeriodStart),
            MONTH(@PeriodStart),
            CASE WHEN @DueDay > DAY(@PeriodEnd) THEN DAY(@PeriodEnd) ELSE @DueDay END) AS StandardDueDate
    ) due
    WHERE a.Status = 'Active'
      AND a.StartDate <= @PeriodEnd
      AND a.EndDate >= @PeriodStart
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.RentCharges c WITH (UPDLOCK, HOLDLOCK)
          WHERE c.AgreementId = a.AgreementId
            AND c.ChargeType = 'MonthlyRent'
            AND c.BillingPeriod = @PeriodStart
      );

    SET @CreatedCount = @@ROWCOUNT;
    COMMIT TRANSACTION;

    SELECT @CreatedCount AS CreatedCount, @PeriodStart AS BillingPeriod, @GenerationRunId AS GenerationRunId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Payment_Post
    @RequestId UNIQUEIDENTIFIER,
    @TenantId INT,
    @AgreementId INT,
    @PaymentDate DATE,
    @Amount DECIMAL(18,2),
    @CurrencyCode CHAR(3),
    @PaymentMethod NVARCHAR(30),
    @ExternalReference NVARCHAR(100) = NULL,
    @Remarks NVARCHAR(300) = NULL,
    @CollectedByUserId INT,
    @Allocations dbo.PaymentAllocationInput READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET ANSI_WARNINGS ON;
    SET ARITHABORT ON;
    SET CONCAT_NULL_YIELDS_NULL ON;
    SET NUMERIC_ROUNDABORT OFF;

    BEGIN TRANSACTION;

    DECLARE @ExistingPaymentId BIGINT;
    SELECT @ExistingPaymentId = PaymentId
    FROM dbo.Payments WITH (UPDLOCK, HOLDLOCK)
    WHERE RequestId = @RequestId;

    IF @ExistingPaymentId IS NOT NULL
    BEGIN
        SELECT PaymentId, ReceiptNo, Amount, CurrencyCode, Status, CAST(1 AS BIT) AS AlreadyProcessed
        FROM dbo.Payments
        WHERE PaymentId = @ExistingPaymentId;
        COMMIT TRANSACTION;
        RETURN;
    END;

    IF @Amount <= 0 THROW 51201, 'Payment amount must be greater than zero.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE UserId = @CollectedByUserId AND IsActive = 1)
        THROW 51202, 'The collector is invalid or inactive.', 1;
    IF NOT EXISTS
    (
        SELECT 1 FROM dbo.RentalAgreements a
        WHERE a.AgreementId = @AgreementId AND a.TenantId = @TenantId
    )
        THROW 51203, 'The tenant and agreement do not match.', 1;
    IF @PaymentMethod NOT IN ('Cash', 'BankTransfer', 'Card', 'MobileBanking', 'Cheque')
        THROW 51204, 'The payment method is invalid.', 1;
    IF @PaymentMethod <> 'Cash' AND NULLIF(LTRIM(RTRIM(@ExternalReference)), '') IS NULL
        THROW 51205, 'An external reference is required for non-cash payments.', 1;
    IF NOT EXISTS (SELECT 1 FROM @Allocations)
        THROW 51206, 'At least one charge allocation is required.', 1;
    IF EXISTS (SELECT 1 FROM @Allocations WHERE Amount <= 0)
        THROW 51207, 'Allocation amounts must be greater than zero.', 1;
    IF (SELECT SUM(Amount) FROM @Allocations) <> @Amount
        THROW 51208, 'Allocation total must equal the payment amount.', 1;

    DECLARE @LockedCharges TABLE
    (
        ChargeId BIGINT PRIMARY KEY,
        AgreementId INT,
        CurrencyCode CHAR(3),
        ChargeStatus NVARCHAR(20),
        OutstandingAmount DECIMAL(18,2)
    );

    INSERT INTO @LockedCharges (ChargeId, AgreementId, CurrencyCode, ChargeStatus, OutstandingAmount)
    SELECT
        c.ChargeId,
        c.AgreementId,
        c.CurrencyCode,
        c.Status,
        c.Amount - ISNULL(SUM(CASE WHEN p.Status = 'Posted' THEN pa.Amount ELSE 0 END), 0)
    FROM dbo.RentCharges c WITH (UPDLOCK, HOLDLOCK)
    INNER JOIN @Allocations input ON input.ChargeId = c.ChargeId
    LEFT JOIN dbo.PaymentAllocations pa WITH (UPDLOCK, HOLDLOCK) ON pa.ChargeId = c.ChargeId
    LEFT JOIN dbo.Payments p WITH (UPDLOCK, HOLDLOCK) ON p.PaymentId = pa.PaymentId
    GROUP BY c.ChargeId, c.AgreementId, c.CurrencyCode, c.Status, c.Amount;

    IF (SELECT COUNT(*) FROM @LockedCharges) <> (SELECT COUNT(*) FROM @Allocations)
        THROW 51209, 'One or more selected charges no longer exist.', 1;
    IF EXISTS (SELECT 1 FROM @LockedCharges WHERE AgreementId <> @AgreementId OR CurrencyCode <> @CurrencyCode OR ChargeStatus <> 'Open')
        THROW 51210, 'Selected charges are not eligible for this payment.', 1;
    IF EXISTS
    (
        SELECT 1
        FROM @Allocations i
        INNER JOIN @LockedCharges c ON c.ChargeId = i.ChargeId
        WHERE i.Amount > c.OutstandingAmount
    )
        THROW 51211, 'A selected balance changed. Refresh and review the allocation.', 1;

    DECLARE @SequenceValue BIGINT = NEXT VALUE FOR dbo.ReceiptNumberSequence;
    DECLARE @ReceiptNo NVARCHAR(50) = CONCAT('RCP-', CONVERT(CHAR(6), @PaymentDate, 112), '-', RIGHT(REPLICATE('0', 8) + CONVERT(VARCHAR(20), @SequenceValue), 8));
    DECLARE @PaymentId BIGINT;

    INSERT INTO dbo.Payments
    (
        ReceiptNo, RequestId, TenantId, AgreementId, PaymentDate, Amount, CurrencyCode,
        PaymentMethod, ExternalReference, Status, Remarks, CollectedByUserId
    )
    VALUES
    (
        @ReceiptNo, @RequestId, @TenantId, @AgreementId, @PaymentDate, @Amount, @CurrencyCode,
        @PaymentMethod, NULLIF(LTRIM(RTRIM(@ExternalReference)), ''), 'Posted', NULLIF(LTRIM(RTRIM(@Remarks)), ''), @CollectedByUserId
    );

    SET @PaymentId = SCOPE_IDENTITY();

    INSERT INTO dbo.PaymentAllocations (PaymentId, ChargeId, Amount)
    SELECT @PaymentId, ChargeId, Amount FROM @Allocations;

    COMMIT TRANSACTION;

    SELECT @PaymentId AS PaymentId, @ReceiptNo AS ReceiptNo, @Amount AS Amount,
           @CurrencyCode AS CurrencyCode, 'Posted' AS Status, CAST(0 AS BIT) AS AlreadyProcessed;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Payment_Reverse
    @PaymentId BIGINT,
    @RequestId UNIQUEIDENTIFIER,
    @Reason NVARCHAR(500),
    @ReversedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET ANSI_WARNINGS ON;
    SET ARITHABORT ON;
    SET CONCAT_NULL_YIELDS_NULL ON;
    SET NUMERIC_ROUNDABORT OFF;
    BEGIN TRANSACTION;

    DECLARE @ExistingPaymentId BIGINT;
    SELECT @ExistingPaymentId = PaymentId
    FROM dbo.PaymentReversals WITH (UPDLOCK, HOLDLOCK)
    WHERE RequestId = @RequestId;

    IF @ExistingPaymentId IS NOT NULL
    BEGIN
        SELECT p.PaymentId, p.ReceiptNo, p.Status, CAST(1 AS BIT) AS AlreadyProcessed
        FROM dbo.Payments p WHERE p.PaymentId = @ExistingPaymentId;
        COMMIT TRANSACTION;
        RETURN;
    END;

    IF NULLIF(LTRIM(RTRIM(@Reason)), '') IS NULL
        THROW 51301, 'A reversal reason is required.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.Users u INNER JOIN dbo.Roles r ON r.RoleId = u.RoleId WHERE u.UserId = @ReversedByUserId AND u.IsActive = 1 AND r.RoleName = 'Admin')
        THROW 51302, 'Only an active administrator can reverse a payment.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.Payments WITH (UPDLOCK, HOLDLOCK) WHERE PaymentId = @PaymentId)
        THROW 51303, 'The selected payment does not exist.', 1;
    IF EXISTS (SELECT 1 FROM dbo.Payments WHERE PaymentId = @PaymentId AND Status <> 'Posted')
        THROW 51304, 'Only a posted payment can be reversed.', 1;

    INSERT INTO dbo.PaymentReversals (PaymentId, RequestId, Reason, ReversedByUserId)
    VALUES (@PaymentId, @RequestId, LTRIM(RTRIM(@Reason)), @ReversedByUserId);

    UPDATE dbo.Payments SET Status = 'Reversed' WHERE PaymentId = @PaymentId;
    COMMIT TRANSACTION;

    SELECT PaymentId, ReceiptNo, Status, CAST(0 AS BIT) AS AlreadyProcessed
    FROM dbo.Payments WHERE PaymentId = @PaymentId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetDashboardSummary
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @PeriodStart DATE = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
    DECLARE @PeriodEnd DATE = EOMONTH(@PeriodStart);

    SELECT
        (SELECT COUNT(*) FROM dbo.Properties WHERE IsActive = 1) AS TotalProperties,
        (SELECT COUNT(*) FROM dbo.Houses WHERE IsActive = 1) AS TotalHouses,
        (SELECT COUNT(*) FROM dbo.Rooms WHERE Status <> 'Inactive') AS TotalRooms,
        (SELECT COUNT(*) FROM dbo.Rooms WHERE Status = 'Available') AS AvailableRooms,
        (SELECT COUNT(*) FROM dbo.Rooms WHERE Status = 'Occupied') AS OccupiedRooms,
        (SELECT COUNT(*) FROM dbo.Tenants WHERE Status = 'Active') AS TotalTenants,
        (SELECT COUNT(*) FROM dbo.RentalAgreements WHERE Status = 'Active') AS ActiveAgreements,
        (SELECT ISNULL(SUM(Amount), 0) FROM dbo.vw_RentChargeBalances WHERE BillingPeriod = @PeriodStart AND ChargeRecordStatus <> 'Waived') AS MonthlyExpectedRent,
        (SELECT ISNULL(SUM(Amount), 0) FROM dbo.Payments WHERE Status = 'Posted' AND PaymentDate BETWEEN @PeriodStart AND @PeriodEnd) AS MonthlyCollectedRent,
        (SELECT ISNULL(SUM(BalanceAmount), 0) FROM dbo.vw_RentChargeBalances WHERE BillingPeriod = @PeriodStart AND ChargeRecordStatus <> 'Waived') AS MonthlyDueRent,
        (SELECT COUNT(*) FROM dbo.vw_RentChargeBalances WHERE ChargeStatus = 'Overdue') AS OverduePayments;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetRentCollectionReport
    @DateFrom DATE,
    @DateTo DATE
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        ReceiptNo, PaymentDate, PostedAt, TenantName, AgreementNo, PropertyName, HouseName, RoomNo,
        Amount AS PaidAmount, CurrencyCode, PaymentMethod, ExternalReference, Status, CollectedByName
    FROM dbo.vw_PaymentHistory
    WHERE PaymentDate BETWEEN @DateFrom AND @DateTo
    ORDER BY PaymentDate DESC, PaymentId DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetTenantPaymentHistory
    @TenantId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        PaymentId, ReceiptNo, AgreementNo, PaymentDate, PostedAt, Amount AS PaidAmount,
        CurrencyCode, PaymentMethod, ExternalReference, Status, CollectedByName,
        ReversalReason, ReversedAt, ReversedByName
    FROM dbo.vw_PaymentHistory
    WHERE TenantId = @TenantId
    ORDER BY PaymentDate DESC, PaymentId DESC;
END;
GO

PRINT 'Migration 20260711_001_PaymentsLedger applied successfully.';
GO
