using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sms.api.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffAttendanceStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "StaffAttendances",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Present");

            migrationBuilder.Sql("UPDATE StaffAttendances SET Status = CASE WHEN IsPresent = 1 THEN 'Present' ELSE 'Absent' END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "StaffAttendances");
        }
    }
}
