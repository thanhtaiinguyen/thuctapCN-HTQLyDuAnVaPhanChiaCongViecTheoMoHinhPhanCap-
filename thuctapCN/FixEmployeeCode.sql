-- Script ?? fix NULL EmployeeCode trong database
-- Ch?y script này trong SQL Server Management Studio ho?c Visual Studio SQL Server Object Explorer

USE [thuctapCN]; -- Thay ??i tên database n?u c?n
GO

-- B??c 1: Xóa unique index n?u t?n t?i (migration có th? ?ã t?o m?t ph?n)
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ApplicationUser_EmployeeCode' AND object_id = OBJECT_ID('AspNetUsers'))
BEGIN
    DROP INDEX [IX_ApplicationUser_EmployeeCode] ON [AspNetUsers];
    PRINT '?ã xóa index IX_ApplicationUser_EmployeeCode';
END
GO

-- B??c 2: C?p nh?t t?t c? EmployeeCode NULL ho?c r?ng
DECLARE @counter INT = 1;
DECLARE @userId NVARCHAR(450);
DECLARE @email NVARCHAR(256);
DECLARE @newCode NVARCHAR(50);

PRINT 'B?t ??u c?p nh?t EmployeeCode...';

-- T?o cursor ?? duy?t qua các user không có EmployeeCode
DECLARE user_cursor CURSOR FOR
SELECT Id, Email FROM AspNetUsers WHERE EmployeeCode IS NULL OR EmployeeCode = '';

OPEN user_cursor;
FETCH NEXT FROM user_cursor INTO @userId, @email;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- T?o mã m?i và ki?m tra trùng
    SET @newCode = 'EMP' + RIGHT('0000' + CAST(@counter AS NVARCHAR), 4);
    
    WHILE EXISTS (SELECT 1 FROM AspNetUsers WHERE EmployeeCode = @newCode)
    BEGIN
        SET @counter = @counter + 1;
        SET @newCode = 'EMP' + RIGHT('0000' + CAST(@counter AS NVARCHAR), 4);
    END;
    
    -- C?p nh?t
    UPDATE AspNetUsers SET EmployeeCode = @newCode WHERE Id = @userId;
    PRINT 'C?p nh?t user ' + @email + ' v?i EmployeeCode: ' + @newCode;
    SET @counter = @counter + 1;
    
    FETCH NEXT FROM user_cursor INTO @userId, @email;
END;

CLOSE user_cursor;
DEALLOCATE user_cursor;

PRINT 'Hoàn thành c?p nh?t EmployeeCode!';
GO

-- B??c 3: Ki?m tra k?t qu?
SELECT Id, Email, EmployeeCode, FullName 
FROM AspNetUsers 
ORDER BY EmployeeCode;
GO

-- B??c 4: T?o l?i unique index
CREATE UNIQUE INDEX [IX_ApplicationUser_EmployeeCode] ON [AspNetUsers] ([EmployeeCode]);
PRINT '?ã t?o l?i unique index IX_ApplicationUser_EmployeeCode';
GO

PRINT 'Script hoàn thành! Bây gi? b?n có th? ch?y l?i ?ng d?ng.';
