-- Check existing employees and their credentials
SELECT 
    COALESCE(EmployeeId, 'Not Set') as 'Employee ID',
    Name,
    Role,
    COALESCE(Category, 'Not Set') as 'Category',
    COALESCE(Email, 'Not Set') as 'Email',
    COALESCE(Phone, 'Not Set') as 'Phone'
FROM Employees 
ORDER BY Id;

-- Show total count
SELECT COUNT(*) as 'Total Employees' FROM Employees;
