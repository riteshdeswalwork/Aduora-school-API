using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sms.api.Migrations
{
    /// <inheritdoc />
    public partial class EnsurePayrollTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[PayrollComponentDefinitions]', N'U') IS NULL
BEGIN
    CREATE TABLE [PayrollComponentDefinitions] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(64) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Kind] int NOT NULL,
        [CalculationMode] int NOT NULL,
        [AmountOrPercent] decimal(18,4) NOT NULL,
        [MaxAmountCap] decimal(18,2) NULL,
        [MinGrossEligible] decimal(18,2) NULL,
        [MaxGrossEligible] decimal(18,2) NULL,
        [SortOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PayrollComponentDefinitions] PRIMARY KEY ([Id])
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PayrollComponentDefinitions_Code' AND object_id = OBJECT_ID(N'[PayrollComponentDefinitions]'))
BEGIN
    CREATE UNIQUE INDEX [IX_PayrollComponentDefinitions_Code] ON [PayrollComponentDefinitions] ([Code]);
END

IF OBJECT_ID(N'[PayrollPeriods]', N'U') IS NULL
BEGIN
    CREATE TABLE [PayrollPeriods] (
        [Id] uniqueidentifier NOT NULL,
        [AcademicYearId] uniqueidentifier NOT NULL,
        [Year] int NOT NULL,
        [Month] int NOT NULL,
        [Status] nvarchar(32) NOT NULL,
        [WorkWeekMode] nvarchar(16) NOT NULL,
        [AssumeFullAttendanceIfNoRecords] bit NOT NULL,
        [Notes] nvarchar(1000) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        CONSTRAINT [PK_PayrollPeriods] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PayrollPeriods_AcademicYears_AcademicYearId] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYears] ([Id]),
        CONSTRAINT [FK_PayrollPeriods_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id])
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PayrollPeriods_AcademicYearId_Year_Month' AND object_id = OBJECT_ID(N'[PayrollPeriods]'))
BEGIN
    CREATE UNIQUE INDEX [IX_PayrollPeriods_AcademicYearId_Year_Month] ON [PayrollPeriods] ([AcademicYearId], [Year], [Month]);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PayrollPeriods_CreatedByUserId' AND object_id = OBJECT_ID(N'[PayrollPeriods]'))
BEGIN
    CREATE INDEX [IX_PayrollPeriods_CreatedByUserId] ON [PayrollPeriods] ([CreatedByUserId]);
END

IF OBJECT_ID(N'[PayrollSlips]', N'U') IS NULL
BEGIN
    CREATE TABLE [PayrollSlips] (
        [Id] uniqueidentifier NOT NULL,
        [PayrollPeriodId] uniqueidentifier NOT NULL,
        [StaffId] uniqueidentifier NOT NULL,
        [AcademicYearId] uniqueidentifier NOT NULL,
        [WorkingDays] int NOT NULL,
        [PresentDays] int NOT NULL,
        [PaidLeaveDays] int NOT NULL,
        [LopDays] int NOT NULL,
        [ProrationFactor] decimal(18,6) NOT NULL,
        [MonthlyBasicFromProfile] decimal(18,2) NOT NULL,
        [GrossTotal] decimal(18,2) NOT NULL,
        [DeductionTotal] decimal(18,2) NOT NULL,
        [NetPay] decimal(18,2) NOT NULL,
        [EarningsJson] nvarchar(max) NOT NULL,
        [DeductionsJson] nvarchar(max) NOT NULL,
        [CalculationTrace] nvarchar(4000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PayrollSlips] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PayrollSlips_PayrollPeriods_PayrollPeriodId] FOREIGN KEY ([PayrollPeriodId]) REFERENCES [PayrollPeriods] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PayrollSlips_Staff_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [Staff] ([Id])
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PayrollSlips_PayrollPeriodId_StaffId' AND object_id = OBJECT_ID(N'[PayrollSlips]'))
BEGIN
    CREATE UNIQUE INDEX [IX_PayrollSlips_PayrollPeriodId_StaffId] ON [PayrollSlips] ([PayrollPeriodId], [StaffId]);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PayrollSlips_StaffId' AND object_id = OBJECT_ID(N'[PayrollSlips]'))
BEGIN
    CREATE INDEX [IX_PayrollSlips_StaffId] ON [PayrollSlips] ([StaffId]);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[PayrollSlips]', N'U') IS NOT NULL DROP TABLE [PayrollSlips];
IF OBJECT_ID(N'[PayrollPeriods]', N'U') IS NOT NULL DROP TABLE [PayrollPeriods];
IF OBJECT_ID(N'[PayrollComponentDefinitions]', N'U') IS NOT NULL DROP TABLE [PayrollComponentDefinitions];
");
        }
    }
}
