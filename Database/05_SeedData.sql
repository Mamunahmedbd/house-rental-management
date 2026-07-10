USE HouseRentalDB;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'Admin')
BEGIN
    INSERT INTO dbo.Roles (RoleName, Description, IsActive)
    VALUES ('Admin', 'Full system access', 1);
END

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'Staff')
BEGIN
    INSERT INTO dbo.Roles (RoleName, Description, IsActive)
    VALUES ('Staff', 'Daily rental operation access', 1);
END

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'admin')
BEGIN
    INSERT INTO dbo.Users
    (
        RoleId,
        FullName,
        Username,
        PasswordHash,
        PasswordSalt,
        Phone,
        Email,
        IsActive
    )
    VALUES
    (
        (SELECT RoleId FROM dbo.Roles WHERE RoleName = 'Admin'),
        'System Administrator',
        'admin',
        CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', 'admin123'), 2),
        NULL,
        NULL,
        NULL,
        1
    );
END

IF NOT EXISTS (SELECT 1 FROM dbo.AppSettings WHERE SettingKey = 'ApplicationName')
BEGIN
    INSERT INTO dbo.AppSettings (SettingKey, SettingValue, Description)
    VALUES
        ('ApplicationName', 'House Rental Management System', 'Application display name'),
        ('DefaultCurrency', 'USD', 'Default currency for reporting'),
        ('RentDueDay', '5', 'Default monthly rent due day'),
        ('ReceiptFooter', 'Thank you for your payment.', 'Receipt footer text');
END
GO
