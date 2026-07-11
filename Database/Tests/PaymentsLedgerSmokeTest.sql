USE HouseRentalDB;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @AdminUserId INT =
    (
        SELECT TOP (1) u.UserId
        FROM dbo.Users u
        INNER JOIN dbo.Roles r ON r.RoleId = u.RoleId
        WHERE u.IsActive = 1 AND r.RoleName = 'Admin'
        ORDER BY u.UserId
    );
    DECLARE @AgreementId INT =
    (
        SELECT TOP (1) AgreementId
        FROM dbo.RentalAgreements
        WHERE Status = 'Active'
        ORDER BY AgreementId
    );
    DECLARE @TenantId INT = (SELECT TenantId FROM dbo.RentalAgreements WHERE AgreementId = @AgreementId);
    DECLARE @BillingPeriod DATE = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
    DECLARE @GenerationRunId UNIQUEIDENTIFIER = NEWID();

    IF @AdminUserId IS NULL THROW 52001, 'Smoke test requires one active Admin user.', 1;
    IF @AgreementId IS NULL THROW 52002, 'Smoke test requires one active RentalAgreement.', 1;

    EXEC dbo.sp_Payment_GenerateMonthlyCharges
        @BillingPeriod = @BillingPeriod,
        @CreatedByUserId = @AdminUserId,
        @GenerationRunId = @GenerationRunId;

    DECLARE @ChargeId BIGINT;
    DECLARE @Balance DECIMAL(18,2);
    DECLARE @Currency CHAR(3);

    SELECT TOP (1)
        @ChargeId = ChargeId,
        @Balance = BalanceAmount,
        @Currency = CurrencyCode
    FROM dbo.vw_RentChargeBalances
    WHERE AgreementId = @AgreementId AND BalanceAmount > 0
    ORDER BY DueDate, ChargeId;

    IF @ChargeId IS NULL THROW 52003, 'Charge generation did not produce an outstanding charge.', 1;
    IF EXISTS
    (
        SELECT 1
        FROM dbo.RentCharges c
        INNER JOIN dbo.RentalAgreements a ON a.AgreementId = c.AgreementId
        WHERE c.BillingPeriod = @BillingPeriod
          AND (c.DueDate < a.StartDate OR c.DueDate > a.EndDate)
    ) THROW 52012, 'A generated charge due date falls outside its agreement term.', 1;

    DECLARE @PaymentRequestId UNIQUEIDENTIFIER = NEWID();
    DECLARE @PaymentAmount DECIMAL(18,2) = CASE WHEN @Balance > 25 THEN 25 ELSE @Balance END;
    DECLARE @Allocations dbo.PaymentAllocationInput;
    INSERT INTO @Allocations (ChargeId, Amount) VALUES (@ChargeId, @PaymentAmount);

    DECLARE @PostResult TABLE
    (
        PaymentId BIGINT,
        ReceiptNo NVARCHAR(50),
        Amount DECIMAL(18,2),
        CurrencyCode CHAR(3),
        Status NVARCHAR(20),
        AlreadyProcessed BIT
    );

    INSERT INTO @PostResult
    EXEC dbo.sp_Payment_Post
        @RequestId = @PaymentRequestId,
        @TenantId = @TenantId,
        @AgreementId = @AgreementId,
        @PaymentDate = @BillingPeriod,
        @Amount = @PaymentAmount,
        @CurrencyCode = @Currency,
        @PaymentMethod = 'Cash',
        @ExternalReference = NULL,
        @Remarks = 'Payments ledger smoke test',
        @CollectedByUserId = @AdminUserId,
        @Allocations = @Allocations;

    DECLARE @PaymentId BIGINT = (SELECT TOP (1) PaymentId FROM @PostResult);
    IF @PaymentId IS NULL THROW 52004, 'Payment posting returned no PaymentId.', 1;
    IF (SELECT Status FROM dbo.Payments WHERE PaymentId = @PaymentId) <> 'Posted'
        THROW 52005, 'Posted payment status is invalid.', 1;
    IF (SELECT COUNT(*) FROM dbo.Payments WHERE RequestId = @PaymentRequestId) <> 1
        THROW 52006, 'Payment request idempotency failed.', 1;

    DELETE FROM @PostResult;
    INSERT INTO @PostResult
    EXEC dbo.sp_Payment_Post
        @RequestId = @PaymentRequestId,
        @TenantId = @TenantId,
        @AgreementId = @AgreementId,
        @PaymentDate = @BillingPeriod,
        @Amount = @PaymentAmount,
        @CurrencyCode = @Currency,
        @PaymentMethod = 'Cash',
        @ExternalReference = NULL,
        @Remarks = 'Idempotent retry',
        @CollectedByUserId = @AdminUserId,
        @Allocations = @Allocations;

    IF NOT EXISTS (SELECT 1 FROM @PostResult WHERE PaymentId = @PaymentId AND AlreadyProcessed = 1)
        THROW 52007, 'Repeated post did not resolve the original result.', 1;

    DECLARE @ReversalRequestId UNIQUEIDENTIFIER = NEWID();
    DECLARE @ReverseResult TABLE
    (
        PaymentId BIGINT,
        ReceiptNo NVARCHAR(50),
        Status NVARCHAR(20),
        AlreadyProcessed BIT
    );

    INSERT INTO @ReverseResult
    EXEC dbo.sp_Payment_Reverse
        @PaymentId = @PaymentId,
        @RequestId = @ReversalRequestId,
        @Reason = 'Smoke-test reversal',
        @ReversedByUserId = @AdminUserId;

    IF (SELECT Status FROM dbo.Payments WHERE PaymentId = @PaymentId) <> 'Reversed'
        THROW 52008, 'Payment reversal did not update status.', 1;
    IF (SELECT COUNT(*) FROM dbo.PaymentReversals WHERE PaymentId = @PaymentId) <> 1
        THROW 52009, 'Payment reversal event was not recorded exactly once.', 1;

    DELETE FROM @ReverseResult;
    INSERT INTO @ReverseResult
    EXEC dbo.sp_Payment_Reverse
        @PaymentId = @PaymentId,
        @RequestId = @ReversalRequestId,
        @Reason = 'Idempotent retry',
        @ReversedByUserId = @AdminUserId;

    IF NOT EXISTS (SELECT 1 FROM @ReverseResult WHERE PaymentId = @PaymentId AND AlreadyProcessed = 1)
        THROW 52010, 'Repeated reversal did not resolve the original result.', 1;

    IF (SELECT BalanceAmount FROM dbo.vw_RentChargeBalances WHERE ChargeId = @ChargeId) <> @Balance
        THROW 52011, 'Reversal did not restore the effective charge balance.', 1;

    ROLLBACK TRANSACTION;
    SELECT 'PASS' AS TestResult, 'Payments ledger post/idempotency/reversal smoke test passed and rolled back.' AS Details;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
