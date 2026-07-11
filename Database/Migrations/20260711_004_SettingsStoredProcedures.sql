USE HouseRentalDB;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- ============================================================
-- sp_Settings_GetAll
-- Returns all settings ordered by SettingKey.
-- ============================================================
CREATE OR ALTER PROCEDURE dbo.sp_Settings_GetAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        SettingId,
        SettingKey,
        SettingValue,
        Description
    FROM dbo.AppSettings
    ORDER BY SettingKey;
END;
GO

-- ============================================================
-- sp_Settings_GetByKey
-- Returns a single setting by its unique key.
-- ============================================================
CREATE OR ALTER PROCEDURE dbo.sp_Settings_GetByKey
    @SettingKey NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        SettingId,
        SettingKey,
        SettingValue,
        Description
    FROM dbo.AppSettings
    WHERE SettingKey = @SettingKey;
END;
GO

-- ============================================================
-- sp_Settings_Update
-- Updates the value of an existing setting.
-- Logs old and new values to AuditLogs.
-- Returns the updated row for confirmation.
-- ============================================================
CREATE OR ALTER PROCEDURE dbo.sp_Settings_Update
    @SettingKey NVARCHAR(100),
    @SettingValue NVARCHAR(300),
    @UpdatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.AppSettings WHERE SettingKey = @SettingKey)
        THROW 52001, 'The specified setting key does not exist.', 1;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.Users u
        INNER JOIN dbo.Roles r ON r.RoleId = u.RoleId
        WHERE u.UserId = @UpdatedByUserId AND u.IsActive = 1 AND r.RoleName = 'Admin'
    )
        THROW 52002, 'Only an active administrator can update application settings.', 1;

    DECLARE @OldValue NVARCHAR(300);
    SELECT @OldValue = SettingValue FROM dbo.AppSettings WHERE SettingKey = @SettingKey;

    UPDATE dbo.AppSettings
    SET SettingValue = @SettingValue
    WHERE SettingKey = @SettingKey;

    -- Audit log the change with old and new values
    INSERT INTO dbo.AuditLogs (UserId, ActionName, TableName, RecordId, Description, CreatedAt)
    VALUES (
        @UpdatedByUserId,
        'Update Setting',
        'AppSettings',
        @SettingKey,
        CONCAT('Changed [', @SettingKey, '] from ''',
               ISNULL(@OldValue, 'NULL'), ''' to ''',
               ISNULL(@SettingValue, 'NULL'), '''.'),
        GETDATE()
    );

    SELECT
        SettingId,
        SettingKey,
        SettingValue,
        Description
    FROM dbo.AppSettings
    WHERE SettingKey = @SettingKey;
END;
GO
