USE HouseRentalDB;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
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
    PaymentYear,
    PaymentMonth,
    TotalDue,
    TotalPaid,
    TotalBalance,
    PaymentCount AS ChargeCount
FROM dbo.vw_RentCollectionSummary;
GO
