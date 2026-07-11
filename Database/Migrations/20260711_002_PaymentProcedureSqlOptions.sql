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

IF OBJECT_ID('dbo.SchemaVersions', 'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaVersions WHERE VersionId = '20260711_002_PaymentProcedureSqlOptions')
BEGIN
    INSERT INTO dbo.SchemaVersions (VersionId, Description)
    VALUES
    (
        '20260711_002_PaymentProcedureSqlOptions',
        'Recreate monthly charge generation with required SQL SET options and term-bounded due dates.'
    );
END;
GO
