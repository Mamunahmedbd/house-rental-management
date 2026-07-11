USE HouseRentalDB;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    PRINT 'Starting Report Procedures Smoke Test...';

    -- 1. Test sp_Rpt_TenantList
    PRINT 'Testing sp_Rpt_TenantList...';
    
    -- Test with defaults
    EXEC dbo.sp_Rpt_TenantList @Status = 'All', @SearchText = '';
    
    -- Test with Active status
    EXEC dbo.sp_Rpt_TenantList @Status = 'Active', @SearchText = '';
    
    -- Test with search text
    EXEC dbo.sp_Rpt_TenantList @Status = 'All', @SearchText = 'Test';

    -- 2. Test sp_Rpt_PropertyOccupancy
    PRINT 'Testing sp_Rpt_PropertyOccupancy...';
    
    -- Test with defaults
    EXEC dbo.sp_Rpt_PropertyOccupancy @PropertyId = NULL, @IncludeInactive = 0;
    
    -- Test including inactive
    EXEC dbo.sp_Rpt_PropertyOccupancy @PropertyId = NULL, @IncludeInactive = 1;

    -- 3. Test sp_Rpt_MonthlyDue
    PRINT 'Testing sp_Rpt_MonthlyDue...';
    
    DECLARE @BillingPeriod DATE = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
    
    -- Test for current month
    EXEC dbo.sp_Rpt_MonthlyDue @BillingPeriod = @BillingPeriod, @ChargeStatus = 'All';
    
    -- Test with Due status filter
    EXEC dbo.sp_Rpt_MonthlyDue @BillingPeriod = @BillingPeriod, @ChargeStatus = 'Due';

    -- 4. Test sp_Rpt_IncomeSummary
    PRINT 'Testing sp_Rpt_IncomeSummary...';
    
    DECLARE @DateFrom DATE = DATEADD(MONTH, -6, GETDATE());
    DECLARE @DateTo DATE = GETDATE();
    
    EXEC dbo.sp_Rpt_IncomeSummary @DateFrom = @DateFrom, @DateTo = @DateTo;

    PRINT 'Report Procedures Smoke Test Completed Successfully!';
    
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();
    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH;
GO
