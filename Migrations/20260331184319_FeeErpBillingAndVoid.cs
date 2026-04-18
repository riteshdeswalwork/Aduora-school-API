using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sms.api.Migrations
{
    /// <inheritdoc />
    public partial class FeeErpBillingAndVoid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BillingPeriodMonth",
                table: "StudentFees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BillingPeriodYear",
                table: "StudentFees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InstallmentSequence",
                table: "StudentFees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "StudentFees",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Manual");

            migrationBuilder.Sql(@"
UPDATE StudentFees
SET BillingPeriodYear = DATEPART(year, DueDate),
    BillingPeriodMonth = DATEPART(month, DueDate),
    InstallmentSequence = 1
WHERE BillingPeriodYear IS NULL AND DueDate IS NOT NULL;");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReprintAtUtc",
                table: "FeeReceipts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReprintCount",
                table: "FeeReceipts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsVoided",
                table: "FeePayments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VoidReason",
                table: "FeePayments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "VoidedAtUtc",
                table: "FeePayments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VoidedById",
                table: "FeePayments",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillingPeriodMonth",
                table: "StudentFees");

            migrationBuilder.DropColumn(
                name: "BillingPeriodYear",
                table: "StudentFees");

            migrationBuilder.DropColumn(
                name: "InstallmentSequence",
                table: "StudentFees");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "StudentFees");

            migrationBuilder.DropColumn(
                name: "LastReprintAtUtc",
                table: "FeeReceipts");

            migrationBuilder.DropColumn(
                name: "ReprintCount",
                table: "FeeReceipts");

            migrationBuilder.DropColumn(
                name: "IsVoided",
                table: "FeePayments");

            migrationBuilder.DropColumn(
                name: "VoidReason",
                table: "FeePayments");

            migrationBuilder.DropColumn(
                name: "VoidedAtUtc",
                table: "FeePayments");

            migrationBuilder.DropColumn(
                name: "VoidedById",
                table: "FeePayments");
        }
    }
}
