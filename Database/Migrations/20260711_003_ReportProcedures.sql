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

IF EXISTS (SELECT 1 FROM dbo.SchemaVersions WHERE VersionId = '20260711_003_ReportProcedures')
BEGIN
    PRINT 'Migration 20260711_003_ReportProcedures already applied.';
END;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- 1. Create or Alter sp_Rpt_TenantList
    EXEC('
    CREATE OR ALTER PROCEDURE dbo.sp_Rpt_TenantList
        @Status NVARCHAR(30) = NULL,
        @SearchText NVARCHAR(150) = NULL
    AS
    BEGIN
        SET NOCOUNT ON;

        SELECT
            t.TenantId,
            t.FullName,
            t.Phone,
            t.Email,
            t.NationalId,
            t.Address,
            t.EmergencyContactName,
            t.EmergencyContactPhone,
            t.Status,
            t.CreatedAt,
            tr.TotalDue,
            tr.TotalPaid,
            tr.TotalBalance,
            tr.OverdueAmount,
            tr.OverdueCount,
            tr.ChargeCount,
            tr.PaymentCount
        FROM dbo.Tenants t
        LEFT JOIN dbo.vw_TenantReceivables tr ON tr.TenantId = t.TenantId
        WHERE
            (@Status IS NULL OR @Status = '''' OR @Status = ''All'' OR t.Status = @Status)
            AND
            (
                @SearchText IS NULL OR @SearchText = ''''
                OR t.FullName LIKE ''%'' + @SearchText + ''%''
                OR t.Phone LIKE ''%'' + @SearchText + ''%''
                OR t.Email LIKE ''%'' + @SearchText + ''%''
                OR t.NationalId LIKE ''%'' + @SearchText + ''%''
            )
        ORDER BY t.FullName;
    END;
    ');

    -- 2. Create or Alter sp_Rpt_PropertyOccupancy
    EXEC('
    CREATE OR ALTER PROCEDURE dbo.sp_Rpt_PropertyOccupancy
        @PropertyId INT = NULL,
        @IncludeInactive BIT = 0
    AS
    BEGIN
        SET NOCOUNT ON;

        SELECT
            o.PropertyId,
            o.PropertyName,
            o.HouseId,
            o.HouseName,
            o.RoomId,
            o.RoomNo,
            o.RoomType,
            o.MonthlyRent,
            o.RoomStatus,
            o.TenantId,
            o.TenantName,
            o.AgreementId,
            o.AgreementNo,
            o.StartDate,
            o.EndDate,
            o.AgreementStatus
        FROM dbo.vw_RoomOccupancy o
        INNER JOIN dbo.Properties p ON p.PropertyId = o.PropertyId
        INNER JOIN dbo.Houses h ON h.HouseId = o.HouseId
        WHERE
            (@PropertyId IS NULL OR o.PropertyId = @PropertyId)
            AND (p.IsActive = 1)
            AND (h.IsActive = 1)
            AND (@IncludeInactive = 1 OR o.RoomStatus <> ''Inactive'')
        ORDER BY o.PropertyName, o.HouseName, o.RoomNo;
    END;
    ');

    -- 3. Create or Alter sp_Rpt_MonthlyDue
    EXEC('
    CREATE OR ALTER PROCEDURE dbo.sp_Rpt_MonthlyDue
        @BillingPeriod DATE,
        @ChargeStatus NVARCHAR(30) = NULL
    AS
    BEGIN
        SET NOCOUNT ON;

        DECLARE @PeriodStart DATE = DATEFROMPARTS(YEAR(@BillingPeriod), MONTH(@BillingPeriod), 1);

        SELECT
            b.ChargeId,
            b.AgreementId,
            a.AgreementNo,
            t.FullName AS TenantName,
            t.Phone AS TenantPhone,
            p.PropertyName,
            h.HouseName,
            r.RoomNo,
            b.ChargeType,
            b.BillingPeriod,
            b.PeriodStart,
            b.PeriodEnd,
            b.DueDate,
            b.Amount,
            b.PaidAmount,
            b.BalanceAmount,
            b.CurrencyCode,
            b.ChargeStatus,
            CASE
                WHEN b.ChargeStatus = ''Overdue''
                THEN DATEDIFF(DAY, b.DueDate, CONVERT(DATE, GETDATE()))
                ELSE 0
            END AS DaysOverdue
        FROM dbo.vw_RentChargeBalances b
        INNER JOIN dbo.RentalAgreements a ON a.AgreementId = b.AgreementId
        INNER JOIN dbo.Tenants t ON t.TenantId = a.TenantId
        INNER JOIN dbo.Rooms r ON r.RoomId = a.RoomId
        INNER JOIN dbo.Houses h ON h.HouseId = r.HouseId
        INNER JOIN dbo.Properties p ON p.PropertyId = h.PropertyId
        WHERE
            b.BillingPeriod = @PeriodStart
            AND (@ChargeStatus IS NULL OR @ChargeStatus = '''' OR @ChargeStatus = ''All'' OR b.ChargeStatus = @ChargeStatus)
        ORDER BY
            CASE b.ChargeStatus
                WHEN ''Overdue'' THEN 1
                WHEN ''Due'' THEN 2
                WHEN ''Partial'' THEN 3
                WHEN ''Paid'' THEN 4
                WHEN ''Waived'' THEN 5
                ELSE 6
            END,
            t.FullName,
            a.AgreementNo;
    END;
    ');

    -- 4. Create or Alter sp_Rpt_IncomeSummary
    EXEC('
    CREATE OR ALTER PROCEDURE dbo.sp_Rpt_IncomeSummary
        @DateFrom DATE,
        @DateTo DATE
    AS
    BEGIN
        SET NOCOUNT ON;

        DECLARE @PeriodFrom DATE = DATEFROMPARTS(YEAR(@DateFrom), MONTH(@DateFrom), 1);
        DECLARE @PeriodTo DATE = EOMONTH(DATEFROMPARTS(YEAR(@DateTo), MONTH(@DateTo), 1));

        WITH MonthSeries AS
        (
            SELECT @PeriodFrom AS PeriodStart
            UNION ALL
            SELECT DATEADD(MONTH, 1, PeriodStart)
            FROM MonthSeries
            WHERE DATEADD(MONTH, 1, PeriodStart) <= DATEFROMPARTS(YEAR(@PeriodTo), MONTH(@PeriodTo), 1)
        ),
        Charges AS
        (
            SELECT
                DATEFROMPARTS(YEAR(BillingPeriod), MONTH(BillingPeriod), 1) AS PeriodStart,
                SUM(CASE WHEN ChargeRecordStatus <> ''Waived'' THEN Amount ELSE 0 END) AS ExpectedRent,
                SUM(CASE WHEN ChargeRecordStatus <> ''Waived'' THEN PaidAmount ELSE 0 END) AS CollectedRent
            FROM dbo.vw_RentChargeBalances
            WHERE BillingPeriod BETWEEN @PeriodFrom AND @PeriodTo
            GROUP BY DATEFROMPARTS(YEAR(BillingPeriod), MONTH(BillingPeriod), 1)
        ),
        ExpenseAgg AS
        (
            SELECT
                DATEFROMPARTS(YEAR(ExpenseDate), MONTH(ExpenseDate), 1) AS PeriodStart,
                SUM(Amount) AS TotalExpenses
            FROM dbo.Expenses
            WHERE ExpenseDate BETWEEN @PeriodFrom AND @PeriodTo
            GROUP BY DATEFROMPARTS(YEAR(ExpenseDate), MONTH(ExpenseDate), 1)
        )
        SELECT
            ms.PeriodStart,
            YEAR(ms.PeriodStart) AS PeriodYear,
            MONTH(ms.PeriodStart) AS PeriodMonth,
            ISNULL(c.ExpectedRent, 0) AS ExpectedRent,
            ISNULL(c.CollectedRent, 0) AS CollectedRent,
            CASE
                WHEN ISNULL(c.ExpectedRent, 0) > 0
                THEN ROUND(ISNULL(c.CollectedRent, 0) * 100.0 / c.ExpectedRent, 2)
                ELSE 0
            END AS CollectionRate,
            ISNULL(e.TotalExpenses, 0) AS TotalExpenses,
            ISNULL(c.CollectedRent, 0) - ISNULL(e.TotalExpenses, 0) AS NetIncome,
            ISNULL(c.ExpectedRent, 0) - ISNULL(c.CollectedRent, 0) AS Variance
        FROM MonthSeries ms
        LEFT JOIN Charges c ON c.PeriodStart = ms.PeriodStart
        LEFT JOIN ExpenseAgg e ON e.PeriodStart = ms.PeriodStart
        ORDER BY ms.PeriodStart
        OPTION (MAXRECURSION 120);
    END;
    ');

    IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersions WHERE VersionId = '20260711_003_ReportProcedures')
    BEGIN
        INSERT INTO dbo.SchemaVersions (VersionId, Description)
        VALUES ('20260711_003_ReportProcedures', 'Added report stored procedures (TenantList, PropertyOccupancy, MonthlyDue, IncomeSummary)');
    END;

    COMMIT TRANSACTION;
    PRINT 'Migration 20260711_003_ReportProcedures completed successfully.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();
    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH;
GO
