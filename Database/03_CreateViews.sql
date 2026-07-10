USE HouseRentalDB;
GO

CREATE OR ALTER VIEW dbo.vw_RoomOccupancy
AS
SELECT
    p.PropertyId,
    p.PropertyName,
    h.HouseId,
    h.HouseName,
    r.RoomId,
    r.RoomNo,
    r.RoomType,
    r.MonthlyRent,
    r.Status AS RoomStatus,
    t.TenantId,
    t.FullName AS TenantName,
    a.AgreementId,
    a.AgreementNo,
    a.StartDate,
    a.EndDate,
    a.Status AS AgreementStatus
FROM dbo.Properties p
INNER JOIN dbo.Houses h ON h.PropertyId = p.PropertyId
INNER JOIN dbo.Rooms r ON r.HouseId = h.HouseId
LEFT JOIN dbo.RentalAgreements a ON a.RoomId = r.RoomId AND a.Status = 'Active'
LEFT JOIN dbo.Tenants t ON t.TenantId = a.TenantId;
GO

CREATE OR ALTER VIEW dbo.vw_ActiveAgreements
AS
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
WHERE a.Status = 'Active';
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
    ISNULL(SUM(rp.DueAmount), 0) AS TotalDue,
    ISNULL(SUM(rp.PaidAmount), 0) AS TotalPaid,
    ISNULL(SUM(rp.BalanceAmount), 0) AS TotalBalance,
    COUNT(rp.PaymentId) AS PaymentCount
FROM dbo.RentalAgreements a
INNER JOIN dbo.Tenants t ON t.TenantId = a.TenantId
INNER JOIN dbo.Rooms r ON r.RoomId = a.RoomId
INNER JOIN dbo.Houses h ON h.HouseId = r.HouseId
INNER JOIN dbo.Properties p ON p.PropertyId = h.PropertyId
INNER JOIN dbo.Users u ON u.UserId = a.CreatedByUserId
LEFT JOIN dbo.RentPayments rp ON rp.AgreementId = a.AgreementId
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
    a.CreatedByUserId,
    u.FullName,
    a.CreatedAt;
GO

CREATE OR ALTER VIEW dbo.vw_RentCollectionSummary
AS
SELECT
    PaymentYear,
    PaymentMonth,
    SUM(DueAmount) AS TotalDue,
    SUM(PaidAmount) AS TotalPaid,
    SUM(BalanceAmount) AS TotalBalance,
    COUNT(*) AS PaymentCount
FROM dbo.RentPayments
GROUP BY PaymentYear, PaymentMonth;
GO

CREATE OR ALTER VIEW dbo.vw_TenantBalances
AS
SELECT
    t.TenantId,
    t.FullName,
    t.Phone,
    SUM(rp.DueAmount) AS TotalDue,
    SUM(rp.PaidAmount) AS TotalPaid,
    SUM(rp.BalanceAmount) AS TotalBalance
FROM dbo.Tenants t
INNER JOIN dbo.RentalAgreements a ON a.TenantId = t.TenantId
INNER JOIN dbo.RentPayments rp ON rp.AgreementId = a.AgreementId
GROUP BY t.TenantId, t.FullName, t.Phone;
GO
