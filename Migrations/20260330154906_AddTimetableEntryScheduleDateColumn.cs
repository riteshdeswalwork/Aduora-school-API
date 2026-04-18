using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sms.api.Migrations
{
    /// <inheritdoc />
    public partial class AddTimetableEntryScheduleDateColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // DBs created before ScheduleDate was in the model need this column. Idempotent if it already exists (e.g. fresh create from older combined migration).
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[TimetableEntries]') AND name = N'ScheduleDate')
BEGIN
    ALTER TABLE [dbo].[TimetableEntries] ADD [ScheduleDate] datetime2 NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[TimetableEntries]') AND name = N'ScheduleDate')
BEGIN
    ALTER TABLE [dbo].[TimetableEntries] DROP COLUMN [ScheduleDate];
END
");
        }
    }
}
