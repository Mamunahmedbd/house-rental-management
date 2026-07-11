USE HouseRentalDB;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRY
    EXEC sys.sp_refreshview 'dbo.vw_RentChargeBalances';
    EXEC sys.sp_refreshview 'dbo.vw_AgreementReceivables';
    EXEC sys.sp_refreshview 'dbo.vw_TenantReceivables';
    EXEC sys.sp_refreshview 'dbo.vw_PaymentHistory';
    EXEC sys.sp_refreshview 'dbo.vw_PaymentAllocationDetails';
    EXEC sys.sp_refreshview 'dbo.vw_AgreementDirectory';
    EXEC sys.sp_refreshview 'dbo.vw_RentCollectionSummary';
    EXEC sys.sp_refreshview 'dbo.vw_TenantBalances';
    EXEC sys.sp_refreshview 'dbo.vw_MonthlyRentCollectionSummary';

    DECLARE @AgreementId INT = (SELECT TOP (1) AgreementId FROM dbo.RentalAgreements ORDER BY AgreementId);
    DECLARE @TenantId INT = (SELECT TenantId FROM dbo.RentalAgreements WHERE AgreementId = @AgreementId);

    SELECT TOP (1) * FROM dbo.vw_AgreementDirectory WHERE AgreementId = @AgreementId;
    SELECT TOP (1) * FROM dbo.vw_AgreementReceivables WHERE AgreementId = @AgreementId;
    SELECT TOP (1) * FROM dbo.vw_TenantReceivables WHERE TenantId = @TenantId;
    SELECT TOP (1) * FROM dbo.vw_RentChargeBalances WHERE AgreementId = @AgreementId;
    SELECT TOP (1) * FROM dbo.vw_PaymentHistory WHERE AgreementId = @AgreementId;

    EXEC dbo.sp_GetDashboardSummary;
    EXEC dbo.sp_GetTenantPaymentHistory @TenantId = @TenantId;
    EXEC dbo.sp_GetRentCollectionReport @DateFrom = '20000101', @DateTo = '99991231';

    -- AgreementRepository payment-aware directory shape.
    SELECT
        a.AgreementId, a.AgreementNo, a.TenantId, t.FullName AS TenantName,
        p.PropertyName, h.HouseName, r.RoomNo, a.Status AS AgreementStatus,
        ar.TotalDue, ar.TotalPaid, ar.TotalBalance, ar.PaymentCount
    FROM dbo.RentalAgreements a
    INNER JOIN dbo.Tenants t ON t.TenantId = a.TenantId
    INNER JOIN dbo.Rooms r ON r.RoomId = a.RoomId
    INNER JOIN dbo.Houses h ON h.HouseId = r.HouseId
    INNER JOIN dbo.Properties p ON p.PropertyId = h.PropertyId
    INNER JOIN dbo.vw_AgreementReceivables ar ON ar.AgreementId = a.AgreementId
    WHERE a.AgreementId = @AgreementId
    GROUP BY a.AgreementId, a.AgreementNo, a.TenantId, t.FullName,
             p.PropertyName, h.HouseName, r.RoomNo, a.Status,
             ar.TotalDue, ar.TotalPaid, ar.TotalBalance, ar.PaymentCount;

    -- TenantRepository payment-aware directory shape.
    SELECT
        t.TenantId, t.FullName, t.Status, a.AgreementNo,
        p.PropertyName, h.HouseName, r.RoomNo,
        tr.TotalDue, tr.TotalPaid, tr.TotalBalance
    FROM dbo.Tenants t
    LEFT JOIN dbo.RentalAgreements a ON a.TenantId = t.TenantId AND a.Status = 'Active'
    LEFT JOIN dbo.Rooms r ON r.RoomId = a.RoomId
    LEFT JOIN dbo.Houses h ON h.HouseId = r.HouseId
    LEFT JOIN dbo.Properties p ON p.PropertyId = h.PropertyId
    INNER JOIN dbo.vw_TenantReceivables tr ON tr.TenantId = t.TenantId
    WHERE t.TenantId = @TenantId
    GROUP BY t.TenantId, t.FullName, t.Status, a.AgreementNo,
             p.PropertyName, h.HouseName, r.RoomNo,
             tr.TotalDue, tr.TotalPaid, tr.TotalBalance;

    DBCC CHECKCONSTRAINTS ('dbo.RentCharges') WITH ALL_CONSTRAINTS;
    DBCC CHECKCONSTRAINTS ('dbo.Payments') WITH ALL_CONSTRAINTS;
    DBCC CHECKCONSTRAINTS ('dbo.PaymentAllocations') WITH ALL_CONSTRAINTS;
    DBCC CHECKCONSTRAINTS ('dbo.PaymentReversals') WITH ALL_CONSTRAINTS;

    SELECT 'PASS' AS TestResult, 'Payment read models, procedures, dependencies, and constraints are valid.' AS Details;
END TRY
BEGIN CATCH
    THROW;
END CATCH;
GO
