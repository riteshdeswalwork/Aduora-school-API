using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sms.api.Migrations
{
    /// <inheritdoc />
    public partial class FixShadowForeignKeysAndNullableFeeClassId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicPerformanceHistories_AcademicYears_AcademicYearId1",
                table: "AcademicPerformanceHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_Books_BookCategories_BookCategoryId",
                table: "Books");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassPromotionHistories_AcademicYears_AcademicYearId1",
                table: "ClassPromotionHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_FeeHistoriesByYear_AcademicYears_AcademicYearId1",
                table: "FeeHistoriesByYear");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentAcademicHistories_AcademicYears_AcademicYearId1",
                table: "StudentAcademicHistories");

            migrationBuilder.DropIndex(
                name: "IX_StudentAcademicHistories_AcademicYearId1",
                table: "StudentAcademicHistories");

            migrationBuilder.DropIndex(
                name: "IX_FeeHistoriesByYear_AcademicYearId1",
                table: "FeeHistoriesByYear");

            migrationBuilder.DropIndex(
                name: "IX_ClassPromotionHistories_AcademicYearId1",
                table: "ClassPromotionHistories");

            migrationBuilder.DropIndex(
                name: "IX_Books_BookCategoryId",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_AcademicPerformanceHistories_AcademicYearId1",
                table: "AcademicPerformanceHistories");

            migrationBuilder.DropColumn(
                name: "AcademicYearId1",
                table: "StudentAcademicHistories");

            migrationBuilder.DropColumn(
                name: "AcademicYearId1",
                table: "FeeHistoriesByYear");

            migrationBuilder.DropColumn(
                name: "AcademicYearId1",
                table: "ClassPromotionHistories");

            migrationBuilder.DropColumn(
                name: "BookCategoryId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "AcademicYearId1",
                table: "AcademicPerformanceHistories");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClassId",
                table: "FeeStructures",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId1",
                table: "StudentAcademicHistories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ClassId",
                table: "FeeStructures",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId1",
                table: "FeeHistoriesByYear",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId1",
                table: "ClassPromotionHistories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BookCategoryId",
                table: "Books",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId1",
                table: "AcademicPerformanceHistories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentAcademicHistories_AcademicYearId1",
                table: "StudentAcademicHistories",
                column: "AcademicYearId1");

            migrationBuilder.CreateIndex(
                name: "IX_FeeHistoriesByYear_AcademicYearId1",
                table: "FeeHistoriesByYear",
                column: "AcademicYearId1");

            migrationBuilder.CreateIndex(
                name: "IX_ClassPromotionHistories_AcademicYearId1",
                table: "ClassPromotionHistories",
                column: "AcademicYearId1");

            migrationBuilder.CreateIndex(
                name: "IX_Books_BookCategoryId",
                table: "Books",
                column: "BookCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicPerformanceHistories_AcademicYearId1",
                table: "AcademicPerformanceHistories",
                column: "AcademicYearId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicPerformanceHistories_AcademicYears_AcademicYearId1",
                table: "AcademicPerformanceHistories",
                column: "AcademicYearId1",
                principalTable: "AcademicYears",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_BookCategories_BookCategoryId",
                table: "Books",
                column: "BookCategoryId",
                principalTable: "BookCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassPromotionHistories_AcademicYears_AcademicYearId1",
                table: "ClassPromotionHistories",
                column: "AcademicYearId1",
                principalTable: "AcademicYears",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FeeHistoriesByYear_AcademicYears_AcademicYearId1",
                table: "FeeHistoriesByYear",
                column: "AcademicYearId1",
                principalTable: "AcademicYears",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentAcademicHistories_AcademicYears_AcademicYearId1",
                table: "StudentAcademicHistories",
                column: "AcademicYearId1",
                principalTable: "AcademicYears",
                principalColumn: "Id");
        }
    }
}
