-- Script to create ClassEnrollments table

-- Create ClassEnrollments table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ClassEnrollments')
BEGIN
    CREATE TABLE [dbo].[ClassEnrollments] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [StudentId] UNIQUEIDENTIFIER NOT NULL,
        [ClassId] UNIQUEIDENTIFIER NOT NULL,
        [EnrollmentDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [AcademicYear] NVARCHAR(20) NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        CONSTRAINT [FK_ClassEnrollments_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ClassEnrollments_Classes_ClassId] FOREIGN KEY ([ClassId]) REFERENCES [dbo].[Classes] ([Id]) ON DELETE CASCADE
    );

    -- Create indexes for foreign keys
    CREATE INDEX [IX_ClassEnrollments_StudentId] ON [dbo].[ClassEnrollments] ([StudentId]);
    CREATE INDEX [IX_ClassEnrollments_ClassId] ON [dbo].[ClassEnrollments] ([ClassId]);

    PRINT 'ClassEnrollments table created successfully.';
END
ELSE
BEGIN
    PRINT 'ClassEnrollments table already exists.';
END

-- Add migration record to __EFMigrationsHistory table if it exists
IF EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20240702000000_AddClassEnrollments')
    BEGIN
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES ('20240702000000_AddClassEnrollments', '9.0.0-preview.4.24267.1');
        
        PRINT 'Migration record added to __EFMigrationsHistory table.';
    END
    ELSE
    BEGIN
        PRINT 'Migration record already exists in __EFMigrationsHistory table.';
    END
END
ELSE
BEGIN
    PRINT 'Warning: __EFMigrationsHistory table does not exist. Migration record not added.';
END

PRINT 'Database update completed successfully.';
