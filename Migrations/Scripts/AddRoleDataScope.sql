-- Add DataScope column to Roles (0=Global, 1=AssignedClasses, 2=OwnOnly)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'Roles') AND name = 'DataScope'
)
BEGIN
    ALTER TABLE Roles ADD DataScope int NOT NULL CONSTRAINT DF_Roles_DataScope DEFAULT 1;
END
GO

-- Sync system role defaults
UPDATE Roles SET DataScope = 0 WHERE Name IN (N'Admin', N'Principal', N'Accountant', N'Librarian', N'Receptionist', N'Transport Manager', N'HR');
UPDATE Roles SET DataScope = 1 WHERE Name IN (N'Teacher', N'Staff');
UPDATE Roles SET DataScope = 2 WHERE Name IN (N'Student', N'Parent', N'Driver');
