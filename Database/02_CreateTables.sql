USE HouseRentalDB;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.sp_Payment_Post', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_Payment_Post;
IF OBJECT_ID('dbo.PaymentReversals', 'U') IS NOT NULL DROP TABLE dbo.PaymentReversals;
IF OBJECT_ID('dbo.PaymentAllocations', 'U') IS NOT NULL DROP TABLE dbo.PaymentAllocations;
IF OBJECT_ID('dbo.Payments', 'U') IS NOT NULL DROP TABLE dbo.Payments;
IF OBJECT_ID('dbo.RentCharges', 'U') IS NOT NULL DROP TABLE dbo.RentCharges;
IF OBJECT_ID('dbo.LegacyRentPayments', 'U') IS NOT NULL DROP TABLE dbo.LegacyRentPayments;
IF OBJECT_ID('dbo.RentPayments', 'U') IS NOT NULL DROP TABLE dbo.RentPayments;
IF TYPE_ID('dbo.PaymentAllocationInput') IS NOT NULL DROP TYPE dbo.PaymentAllocationInput;
IF OBJECT_ID('dbo.ReceiptNumberSequence', 'SO') IS NOT NULL DROP SEQUENCE dbo.ReceiptNumberSequence;
IF OBJECT_ID('dbo.RentalAgreements', 'U') IS NOT NULL DROP TABLE dbo.RentalAgreements;
IF OBJECT_ID('dbo.Expenses', 'U') IS NOT NULL DROP TABLE dbo.Expenses;
IF OBJECT_ID('dbo.MaintenanceRequests', 'U') IS NOT NULL DROP TABLE dbo.MaintenanceRequests;
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL DROP TABLE dbo.AuditLogs;
IF OBJECT_ID('dbo.Tenants', 'U') IS NOT NULL DROP TABLE dbo.Tenants;
IF OBJECT_ID('dbo.Rooms', 'U') IS NOT NULL DROP TABLE dbo.Rooms;
IF OBJECT_ID('dbo.Houses', 'U') IS NOT NULL DROP TABLE dbo.Houses;
IF OBJECT_ID('dbo.Properties', 'U') IS NOT NULL DROP TABLE dbo.Properties;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL DROP TABLE dbo.Roles;
IF OBJECT_ID('dbo.AppSettings', 'U') IS NOT NULL DROP TABLE dbo.AppSettings;
GO

CREATE TABLE dbo.Roles
(
    RoleId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL CONSTRAINT UQ_Roles_RoleName UNIQUE,
    Description NVARCHAR(200) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Roles_IsActive DEFAULT (1)
);

CREATE TABLE dbo.Users
(
    UserId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    RoleId INT NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Username NVARCHAR(50) NOT NULL CONSTRAINT UQ_Users_Username UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    PasswordSalt NVARCHAR(255) NULL,
    Phone NVARCHAR(30) NULL,
    Email NVARCHAR(100) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    LastLoginAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (GETDATE()),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId)
);

CREATE TABLE dbo.Properties
(
    PropertyId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Properties PRIMARY KEY,
    PropertyName NVARCHAR(100) NOT NULL,
    Address NVARCHAR(250) NULL,
    City NVARCHAR(80) NULL,
    Description NVARCHAR(300) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Properties_IsActive DEFAULT (1),
    CreatedAt DATETIME NOT NULL CONSTRAINT DF_Properties_CreatedAt DEFAULT (GETDATE())
);

CREATE TABLE dbo.Houses
(
    HouseId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Houses PRIMARY KEY,
    PropertyId INT NOT NULL,
    HouseName NVARCHAR(100) NOT NULL,
    FloorNo NVARCHAR(20) NULL,
    Description NVARCHAR(300) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Houses_IsActive DEFAULT (1),
    CreatedAt DATETIME NOT NULL CONSTRAINT DF_Houses_CreatedAt DEFAULT (GETDATE()),
    CONSTRAINT FK_Houses_Properties FOREIGN KEY (PropertyId) REFERENCES dbo.Properties(PropertyId)
);

CREATE TABLE dbo.Rooms
(
    RoomId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Rooms PRIMARY KEY,
    HouseId INT NOT NULL,
    RoomNo NVARCHAR(50) NOT NULL,
    RoomType NVARCHAR(50) NULL,
    MonthlyRent DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Rooms_Status DEFAULT ('Available'),
    Description NVARCHAR(300) NULL,
    CreatedAt DATETIME NOT NULL CONSTRAINT DF_Rooms_CreatedAt DEFAULT (GETDATE()),
    CONSTRAINT FK_Rooms_Houses FOREIGN KEY (HouseId) REFERENCES dbo.Houses(HouseId),
    CONSTRAINT CK_Rooms_MonthlyRent CHECK (MonthlyRent > 0),
    CONSTRAINT CK_Rooms_Status CHECK (Status IN ('Available', 'Occupied', 'Maintenance', 'Inactive'))
);

CREATE UNIQUE INDEX UX_Properties_PropertyName_City
ON dbo.Properties(PropertyName, City)
WHERE IsActive = 1;

CREATE UNIQUE INDEX UX_Houses_PropertyId_HouseName
ON dbo.Houses(PropertyId, HouseName)
WHERE IsActive = 1;

CREATE UNIQUE INDEX UX_Rooms_HouseId_RoomNo
ON dbo.Rooms(HouseId, RoomNo)
WHERE Status <> 'Inactive';

CREATE TABLE dbo.Tenants
(
    TenantId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Tenants PRIMARY KEY,
    FullName NVARCHAR(120) NOT NULL,
    Phone NVARCHAR(30) NOT NULL,
    Email NVARCHAR(100) NULL,
    NationalId NVARCHAR(80) NULL,
    Address NVARCHAR(250) NULL,
    EmergencyContactName NVARCHAR(100) NULL,
    EmergencyContactPhone NVARCHAR(30) NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Tenants_Status DEFAULT ('Active'),
    CreatedAt DATETIME NOT NULL CONSTRAINT DF_Tenants_CreatedAt DEFAULT (GETDATE()),
    CONSTRAINT CK_Tenants_Status CHECK (Status IN ('Active', 'Inactive', 'Blacklisted'))
);

CREATE UNIQUE INDEX UX_Tenants_NationalId
ON dbo.Tenants(NationalId)
WHERE NationalId IS NOT NULL AND NationalId <> '';

CREATE INDEX IX_Tenants_Status_FullName
ON dbo.Tenants(Status, FullName);

CREATE INDEX IX_Tenants_Phone
ON dbo.Tenants(Phone);

CREATE TABLE dbo.RentalAgreements
(
    AgreementId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RentalAgreements PRIMARY KEY,
    AgreementNo NVARCHAR(50) NOT NULL CONSTRAINT UQ_RentalAgreements_AgreementNo UNIQUE,
    TenantId INT NOT NULL,
    RoomId INT NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    MonthlyRent DECIMAL(18,2) NOT NULL,
    SecurityDeposit DECIMAL(18,2) NOT NULL CONSTRAINT DF_RentalAgreements_SecurityDeposit DEFAULT (0),
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_RentalAgreements_Status DEFAULT ('Draft'),
    Notes NVARCHAR(500) NULL,
    CreatedByUserId INT NOT NULL,
    CreatedAt DATETIME NOT NULL CONSTRAINT DF_RentalAgreements_CreatedAt DEFAULT (GETDATE()),
    CONSTRAINT FK_RentalAgreements_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT FK_RentalAgreements_Rooms FOREIGN KEY (RoomId) REFERENCES dbo.Rooms(RoomId),
    CONSTRAINT FK_RentalAgreements_Users FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT CK_RentalAgreements_Dates CHECK (EndDate > StartDate),
    CONSTRAINT CK_RentalAgreements_MonthlyRent CHECK (MonthlyRent > 0),
    CONSTRAINT CK_RentalAgreements_Status CHECK (Status IN ('Draft', 'Active', 'Expired', 'Terminated', 'Cancelled'))
);

CREATE UNIQUE INDEX UX_RentalAgreements_OneActiveRoom
ON dbo.RentalAgreements(RoomId)
WHERE Status = 'Active';

CREATE INDEX IX_RentalAgreements_Status_EndDate
ON dbo.RentalAgreements(Status, EndDate);

CREATE INDEX IX_RentalAgreements_TenantId_Status
ON dbo.RentalAgreements(TenantId, Status);

CREATE INDEX IX_RentalAgreements_RoomId_Status
ON dbo.RentalAgreements(RoomId, Status);

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

CREATE SEQUENCE dbo.ReceiptNumberSequence
AS BIGINT START WITH 1 INCREMENT BY 1 CACHE 20;
GO

CREATE TYPE dbo.PaymentAllocationInput AS TABLE
(
    ChargeId BIGINT NOT NULL PRIMARY KEY,
    Amount DECIMAL(18,2) NOT NULL
);
GO

CREATE TABLE dbo.MaintenanceRequests
(
    MaintenanceId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MaintenanceRequests PRIMARY KEY,
    PropertyId INT NULL,
    RoomId INT NULL,
    Title NVARCHAR(150) NOT NULL,
    Description NVARCHAR(500) NULL,
    Cost DECIMAL(18,2) NOT NULL CONSTRAINT DF_MaintenanceRequests_Cost DEFAULT (0),
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_MaintenanceRequests_Status DEFAULT ('Open'),
    RequestedAt DATETIME NOT NULL CONSTRAINT DF_MaintenanceRequests_RequestedAt DEFAULT (GETDATE()),
    CompletedAt DATETIME NULL,
    CONSTRAINT FK_MaintenanceRequests_Properties FOREIGN KEY (PropertyId) REFERENCES dbo.Properties(PropertyId),
    CONSTRAINT FK_MaintenanceRequests_Rooms FOREIGN KEY (RoomId) REFERENCES dbo.Rooms(RoomId),
    CONSTRAINT CK_MaintenanceRequests_Status CHECK (Status IN ('Open', 'In Progress', 'Completed', 'Cancelled'))
);

CREATE TABLE dbo.Expenses
(
    ExpenseId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Expenses PRIMARY KEY,
    PropertyId INT NULL,
    ExpenseDate DATE NOT NULL,
    Category NVARCHAR(80) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(300) NULL,
    CreatedByUserId INT NOT NULL,
    CreatedAt DATETIME NOT NULL CONSTRAINT DF_Expenses_CreatedAt DEFAULT (GETDATE()),
    CONSTRAINT FK_Expenses_Properties FOREIGN KEY (PropertyId) REFERENCES dbo.Properties(PropertyId),
    CONSTRAINT FK_Expenses_Users FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT CK_Expenses_Amount CHECK (Amount >= 0)
);

CREATE TABLE dbo.AuditLogs
(
    AuditLogId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY,
    UserId INT NULL,
    ActionName NVARCHAR(100) NOT NULL,
    TableName NVARCHAR(100) NULL,
    RecordId NVARCHAR(50) NULL,
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT (GETDATE()),
    CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
);

CREATE TABLE dbo.AppSettings
(
    SettingId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AppSettings PRIMARY KEY,
    SettingKey NVARCHAR(100) NOT NULL CONSTRAINT UQ_AppSettings_SettingKey UNIQUE,
    SettingValue NVARCHAR(300) NULL,
    Description NVARCHAR(300) NULL
);
GO
