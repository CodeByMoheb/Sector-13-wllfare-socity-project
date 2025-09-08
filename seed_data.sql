-- Seed Attendance System Data
-- This script will create shifts and update existing employees

-- 1. Create default shifts
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

-- 2. Update existing employees with EmployeeId and Category
DECLARE @counter INT = 1;
DECLARE @empId INT;

DECLARE emp_cursor CURSOR FOR 
SELECT Id FROM Employees ORDER BY Id;

OPEN emp_cursor;
FETCH NEXT FROM emp_cursor INTO @empId;

WHILE @@FETCH_STATUS = 0
BEGIN
    UPDATE Employees 
    SET EmployeeId = 'EMP' + RIGHT('0000' + CAST(@counter AS VARCHAR(4)), 4),
        Category = CASE 
            WHEN Role IN ('অফিস ম্যানেজার', 'কম্পিউটার অপারেটর', 'অফিস সহকারী') THEN 'Office Staff'
            WHEN Role IN ('মাঠ সুপারভাইজার', 'কমান্ডার', 'সহঃ কমান্ডার', 'গার্ড') THEN 'Field Staff'
            WHEN Role IN ('কালেক্টর', 'মালি', 'পিয়ন') THEN 'Support Staff'
            ELSE 'General'
        END
    WHERE Id = @empId;
    
    SET @counter = @counter + 1;
    FETCH NEXT FROM emp_cursor INTO @empId;
END

CLOSE emp_cursor;
DEALLOCATE emp_cursor;

-- 3. Assign shifts to employees
UPDATE Employees 
SET ShiftId = (SELECT ShiftId FROM Shifts WHERE Name = 'Day Shift')
WHERE Role IN ('অফিস ম্যানেজার', 'কম্পিউটার অপারেটর', 'অফিস সহকারী');

UPDATE Employees 
SET ShiftId = (SELECT ShiftId FROM Shifts WHERE Name = 'Morning Shift')
WHERE Role IN ('মাঠ সুপারভাইজার', 'কমান্ডার', 'সহঃ কমান্ডার', 'গার্ড', 'কালেক্টর', 'মালি', 'পিয়ন');

-- 4. Show employee login credentials
PRINT '🎉 Attendance system data seeding completed successfully!';
PRINT '';
PRINT 'Employee Login Credentials:';
PRINT '==========================';

SELECT 
    EmployeeId as 'Employee ID',
    Name,
    Role,
    Category,
    CASE 
        WHEN Role IN ('অফিস ম্যানেজার', 'কম্পিউটার অপারেটর', 'অফিস সহকারী') THEN 'Day Shift (9 AM - 5 PM)'
        WHEN Role IN ('মাঠ সুপারভাইজার', 'কমান্ডার', 'সহঃ কমান্ডার', 'গার্ড', 'কালেক্টর', 'মালি', 'পিয়ন') THEN 'Morning Shift (8 AM - 4 PM)'
        ELSE 'No Shift Assigned'
    END as 'Assigned Shift'
FROM Employees 
ORDER BY EmployeeId;

PRINT '';
PRINT 'Note: For now, any password will work for employee login (password validation needs to be implemented).';
PRINT 'Access the employee login at: /EmployeeAttendance/Login';
