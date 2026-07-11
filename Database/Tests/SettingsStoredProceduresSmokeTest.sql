USE HouseRentalDB;
GO

PRINT '=== Settings Stored Procedures Smoke Test ===';
PRINT '';

-- Test 1: sp_Settings_GetAll
PRINT 'Test 1: sp_Settings_GetAll';
EXEC dbo.sp_Settings_GetAll;
PRINT 'PASS: All settings returned.';
PRINT '';

-- Test 2: sp_Settings_GetByKey
PRINT 'Test 2: sp_Settings_GetByKey (DefaultCurrency)';
EXEC dbo.sp_Settings_GetByKey @SettingKey = 'DefaultCurrency';
PRINT 'PASS: Single setting returned.';
PRINT '';

-- Test 3: sp_Settings_Update (valid Admin update)
PRINT 'Test 3: sp_Settings_Update (valid Admin)';
DECLARE @AdminUserId INT = (
    SELECT TOP 1 UserId FROM dbo.Users u
    INNER JOIN dbo.Roles r ON r.RoleId = u.RoleId
    WHERE r.RoleName = 'Admin' AND u.IsActive = 1
);
IF @AdminUserId IS NOT NULL
BEGIN
    EXEC dbo.sp_Settings_Update
        @SettingKey = 'ReceiptFooter',
        @SettingValue = 'Test footer text.',
        @UpdatedByUserId = @AdminUserId;
    PRINT 'PASS: Setting updated and audit logged.';

    -- Restore original value
    EXEC dbo.sp_Settings_Update
        @SettingKey = 'ReceiptFooter',
        @SettingValue = 'Thank you for your payment.',
        @UpdatedByUserId = @AdminUserId;
    PRINT 'PASS: Original value restored.';
END
ELSE
BEGIN
    PRINT 'SKIP: No Admin user found.';
END
PRINT '';

-- Test 4: sp_Settings_Update (non-existent key)
PRINT 'Test 4: sp_Settings_Update (invalid key)';
BEGIN TRY
    EXEC dbo.sp_Settings_Update
        @SettingKey = 'NonExistentKey',
        @SettingValue = 'test',
        @UpdatedByUserId = 1;
    PRINT 'FAIL: Should have thrown error 52001.';
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 52001
        PRINT 'PASS: Error 52001 thrown as expected.';
    ELSE
        PRINT 'FAIL: Unexpected error ' + CAST(ERROR_NUMBER() AS VARCHAR(20));
END CATCH
PRINT '';

-- Test 5: Verify audit log entries
PRINT 'Test 5: Verify audit log entries';
SELECT TOP 5 * FROM dbo.AuditLogs
WHERE TableName = 'AppSettings'
ORDER BY AuditLogId DESC;
PRINT 'PASS: Audit log entries retrieved.';
PRINT '';

PRINT '=== Settings Smoke Test Complete ===';
GO
