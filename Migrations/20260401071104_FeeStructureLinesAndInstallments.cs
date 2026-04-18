using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sms.api.Migrations
{
    /// <inheritdoc />
    public partial class FeeStructureLinesAndInstallments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FeeStructureLineId",
                table: "StudentFees",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InstallmentCount",
                table: "FeeStructures",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FeeStructureLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeeStructureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    DefaultConcessionPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeStructureLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeStructureLines_FeeStructures_FeeStructureId",
                        column: x => x.FeeStructureId,
                        principalTable: "FeeStructures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentFees_FeeStructureLineId",
                table: "StudentFees",
                column: "FeeStructureLineId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeStructureLines_FeeStructureId",
                table: "FeeStructureLines",
                column: "FeeStructureId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentFees_FeeStructureLines_FeeStructureLineId",
                table: "StudentFees",
                column: "FeeStructureLineId",
                principalTable: "FeeStructureLines",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentFees_FeeStructureLines_FeeStructureLineId",
                table: "StudentFees");

            migrationBuilder.DropTable(
                name: "FeeStructureLines");

            migrationBuilder.DropIndex(
                name: "IX_StudentFees_FeeStructureLineId",
                table: "StudentFees");

            migrationBuilder.DropColumn(
                name: "FeeStructureLineId",
                table: "StudentFees");

            migrationBuilder.DropColumn(
                name: "InstallmentCount",
                table: "FeeStructures");
        }
    }
}
