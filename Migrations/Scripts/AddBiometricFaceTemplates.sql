-- Face recognition embeddings (client-side face-api / @vladmandic/face-api)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BiometricFaceTemplates')
BEGIN
    CREATE TABLE [dbo].[BiometricFaceTemplates] (
        [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_BiometricFaceTemplates] PRIMARY KEY,
        [PersonType] NVARCHAR(20) NOT NULL,
        [StaffId] UNIQUEIDENTIFIER NULL,
        [StudentId] UNIQUEIDENTIFIER NULL,
        [SlotIndex] INT NOT NULL,
        [EmbeddingJson] NVARCHAR(MAX) NOT NULL,
        [ModelVersion] NVARCHAR(80) NOT NULL CONSTRAINT [DF_BiometricFaceTemplates_ModelVersion] DEFAULT (N'face-api-ssd-mobilenet-v1'),
        [Quality] INT NOT NULL CONSTRAINT [DF_BiometricFaceTemplates_Quality] DEFAULT (0),
        [ThumbnailBase64] NVARCHAR(MAX) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_BiometricFaceTemplates_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [FK_BiometricFaceTemplates_Staff_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [dbo].[Staff] ([Id]),
        CONSTRAINT [FK_BiometricFaceTemplates_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([Id]),
        CONSTRAINT [CK_BiometricFaceTemplates_SlotIndex] CHECK ([SlotIndex] >= 1 AND [SlotIndex] <= 3)
    );

    CREATE INDEX [IX_BiometricFaceTemplates_StaffId] ON [dbo].[BiometricFaceTemplates] ([StaffId]);
    CREATE INDEX [IX_BiometricFaceTemplates_StudentId] ON [dbo].[BiometricFaceTemplates] ([StudentId]);
    CREATE INDEX [IX_BiometricFaceTemplates_IsActive_PersonType] ON [dbo].[BiometricFaceTemplates] ([IsActive], [PersonType]);
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260727120000_AddBiometricFaceTemplates')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260727120000_AddBiometricFaceTemplates', N'9.0.6');
END
GO
