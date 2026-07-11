USE HouseRentalDB;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
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
          SELECT 1 FROM dbo.RentCharges c WITH (UPDLOCK, HOLDLOCK)
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
        FROM dbo.Payments WHERE PaymentId = @ExistingPaymentId;
        COMMIT TRANSACTION;
        RETURN;
    END;

    IF @Amount <= 0 THROW 51201, 'Payment amount must be greater than zero.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE UserId = @CollectedByUserId AND IsActive = 1)
        THROW 51202, 'The collector is invalid or inactive.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.RentalAgreements WHERE AgreementId = @AgreementId AND TenantId = @TenantId)
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

    INSERT INTO @LockedCharges
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
        SELECT 1 FROM @Allocations i
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

    IF NULLIF(LTRIM(RTRIM(@Reason)), '') IS NULL THROW 51301, 'A reversal reason is required.', 1;
    IF NOT EXISTS
    (
        SELECT 1 FROM dbo.Users u
        INNER JOIN dbo.Roles r ON r.RoleId = u.RoleId
        WHERE u.UserId = @ReversedByUserId AND u.IsActive = 1 AND r.RoleName = 'Admin'
    ) THROW 51302, 'Only an active administrator can reverse a payment.', 1;
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
