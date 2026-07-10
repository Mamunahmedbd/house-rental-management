USE HouseRentalDB;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetDashboardSummary
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        (SELECT COUNT(*) FROM dbo.Properties WHERE IsActive = 1) AS TotalProperties,
        (SELECT COUNT(*) FROM dbo.Houses WHERE IsActive = 1) AS TotalHouses,
        (SELECT COUNT(*) FROM dbo.Rooms WHERE Status <> 'Inactive') AS TotalRooms,
        (SELECT COUNT(*) FROM dbo.Rooms WHERE Status = 'Available') AS AvailableRooms,
        (SELECT COUNT(*) FROM dbo.Rooms WHERE Status = 'Occupied') AS OccupiedRooms,
        (SELECT COUNT(*) FROM dbo.Tenants WHERE Status = 'Active') AS TotalTenants,
        (SELECT COUNT(*) FROM dbo.RentalAgreements WHERE Status = 'Active') AS ActiveAgreements,
        (SELECT ISNULL(SUM(MonthlyRent), 0) FROM dbo.RentalAgreements WHERE Status = 'Active') AS MonthlyExpectedRent,
        (SELECT ISNULL(SUM(PaidAmount), 0) FROM dbo.RentPayments WHERE PaymentMonth = MONTH(GETDATE()) AND PaymentYear = YEAR(GETDATE())) AS MonthlyCollectedRent,
        (SELECT ISNULL(SUM(BalanceAmount), 0) FROM dbo.RentPayments WHERE PaymentMonth = MONTH(GETDATE()) AND PaymentYear = YEAR(GETDATE())) AS MonthlyDueRent,
        (SELECT COUNT(*) FROM dbo.RentPayments WHERE Status = 'Overdue') AS OverduePayments;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetRentCollectionReport
    @DateFrom DATE,
    @DateTo DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        rp.ReceiptNo,
        rp.PaymentDate,
        t.FullName AS TenantName,
        p.PropertyName,
        h.HouseName,
        r.RoomNo,
        rp.DueAmount,
        rp.PaidAmount,
        rp.BalanceAmount,
        rp.PaymentMethod,
        rp.Status
    FROM dbo.RentPayments rp
    INNER JOIN dbo.RentalAgreements a ON a.AgreementId = rp.AgreementId
    INNER JOIN dbo.Tenants t ON t.TenantId = a.TenantId
    INNER JOIN dbo.Rooms r ON r.RoomId = a.RoomId
    INNER JOIN dbo.Houses h ON h.HouseId = r.HouseId
    INNER JOIN dbo.Properties p ON p.PropertyId = h.PropertyId
    WHERE rp.PaymentDate BETWEEN @DateFrom AND @DateTo
    ORDER BY rp.PaymentDate DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetTenantPaymentHistory
    @TenantId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        rp.ReceiptNo,
        rp.PaymentMonth,
        rp.PaymentYear,
        rp.DueAmount,
        rp.PaidAmount,
        rp.BalanceAmount,
        rp.PaymentDate,
        rp.PaymentMethod,
        rp.Status
    FROM dbo.RentPayments rp
    INNER JOIN dbo.RentalAgreements a ON a.AgreementId = rp.AgreementId
    WHERE a.TenantId = @TenantId
    ORDER BY rp.PaymentYear DESC, rp.PaymentMonth DESC;
END;
GO
