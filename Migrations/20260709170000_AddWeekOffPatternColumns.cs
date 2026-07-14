using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sms.api.Migrations
{
    public partial class AddWeekOffPatternColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('WeekOffs', 'PatternType') IS NULL
BEGIN
    ALTER TABLE [WeekOffs] ADD [PatternType] nvarchar(20) NOT NULL CONSTRAINT [DF_WeekOffs_PatternType] DEFAULT N'EveryWeek';
END

IF COL_LENGTH('WeekOffs', 'WeekOccurrences') IS NULL
BEGIN
    ALTER TABLE [WeekOffs] ADD [WeekOccurrences] nvarchar(50) NOT NULL CONSTRAINT [DF_WeekOffs_WeekOccurrences] DEFAULT N'';
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('WeekOffs', 'WeekOccurrences') IS NOT NULL
    ALTER TABLE [WeekOffs] DROP CONSTRAINT [DF_WeekOffs_WeekOccurrences];
IF COL_LENGTH('WeekOffs', 'WeekOccurrences') IS NOT NULL
    ALTER TABLE [WeekOffs] DROP COLUMN [WeekOccurrences];
IF COL_LENGTH('WeekOffs', 'PatternType') IS NOT NULL
    ALTER TABLE [DF_WeekOffs_PatternType] DROP CONSTRAINT [DF_WeekOffs_PatternType];
IF COL_LENGTH('WeekOffs', 'PatternType') IS NOT NULL
    ALTER TABLE [WeekOffs] DROP COLUMN [PatternType];
");
        }
    }
}
