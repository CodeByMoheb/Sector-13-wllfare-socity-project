-- Add password fields to Employees table
-- This script adds the necessary password fields for employee authentication

-- Add PasswordHash column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Employees]') AND name = 'PasswordHash')
BEGIN
    ALTER TABLE [Employees] ADD [PasswordHash] nvarchar(100) NULL;
    PRINT '✅ Added PasswordHash column to Employees table';
END
ELSE
BEGIN
    PRINT '⚠️ PasswordHash column already exists';
END

-- Add PasswordSalt column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Employees]') AND name = 'PasswordSalt')
BEGIN
    ALTER TABLE [Employees] ADD [PasswordSalt] nvarchar(100) NULL;
    PRINT '✅ Added PasswordSalt column to Employees table';
END
ELSE
BEGIN
    PRINT '⚠️ PasswordSalt column already exists';
END

-- Create default shifts if they don't exist
IF NOT EXISTS (SELECT 1 FROM Shifts)
BEGIN
    INSERT INTO Shifts (Name, StartTime, EndTime, Description, IsActive, CreatedAt) VALUES
    ('Morning Shift', '08:00:00', '16:00:00', '8 AM to 4 PM', 1, GETDATE()),
    ('Evening Shift', '16:00:00', '00:00:00', '4 PM to 12 AM', 1, GETDATE()),
    ('Night Shift', '00:00:00', '08:00:00', '12 AM to 8 AM', 1, GETDATE()),
    ('Day Shift', '09:00:00', '17:00:00', '9 AM to 5 PM', 1, GETDATE());
    
    PRINT '✅ Created 4 default shifts';
END
ELSE
BEGIN
    PRINT '⚠️ Shifts already exist';
END

-- Show current employee credentials
PRINT '';
PRINT 'Current Employee Credentials:';
PRINT '============================';

SELECT 
    COALESCE(EmployeeId, 'Not Set') as 'Employee ID',
    Name,
    Role,
    COALESCE(Category, 'Not Set') as 'Category',
    CASE 
        WHEN PasswordHash IS NULL THEN 'Default: 123456'
        ELSE 'Password Set'
    END as 'Password Status'
FROM Employees 
ORDER BY Id;

PRINT '';
PRINT 'Employee Login Instructions:';
PRINT '==========================';
PRINT '1. Access: /EmployeeAttendance/Login';
PRINT '2. Use Employee ID and default password: 123456';
PRINT '3. Employees can change their password after first login';
PRINT '4. Managers can reset passwords from Employee Management';
