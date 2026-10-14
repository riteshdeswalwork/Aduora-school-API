using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sms.api.Migrations
{
    /// <inheritdoc />
    public partial class FixLibraryShadowForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookIssues_Books_BookId1",
                table: "BookIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_BookIssues_LibraryMembers_LibraryMemberId1",
                table: "BookIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_BookReservations_Books_BookId1",
                table: "BookReservations");

            migrationBuilder.DropForeignKey(
                name: "FK_BookReservations_LibraryMembers_LibraryMemberId1",
                table: "BookReservations");

            migrationBuilder.DropForeignKey(
                name: "FK_BookReviews_Books_BookId1",
                table: "BookReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Books_Publishers_PublisherId1",
                table: "Books");

            migrationBuilder.DropForeignKey(
                name: "FK_LibraryFines_BookIssues_BookIssueId1",
                table: "LibraryFines");

            migrationBuilder.DropForeignKey(
                name: "FK_LibraryFines_LibraryMembers_LibraryMemberId1",
                table: "LibraryFines");

            migrationBuilder.DropIndex(
                name: "IX_LibraryFines_BookIssueId1",
                table: "LibraryFines");

            migrationBuilder.DropIndex(
                name: "IX_LibraryFines_LibraryMemberId1",
                table: "LibraryFines");

            migrationBuilder.DropIndex(
                name: "IX_Books_PublisherId1",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_BookReviews_BookId1",
                table: "BookReviews");

            migrationBuilder.DropIndex(
                name: "IX_BookReservations_BookId1",
                table: "BookReservations");

            migrationBuilder.DropIndex(
                name: "IX_BookReservations_LibraryMemberId1",
                table: "BookReservations");

            migrationBuilder.DropIndex(
                name: "IX_BookIssues_BookId1",
                table: "BookIssues");

            migrationBuilder.DropIndex(
                name: "IX_BookIssues_LibraryMemberId1",
                table: "BookIssues");

            migrationBuilder.DropColumn(
                name: "BookIssueId1",
                table: "LibraryFines");

            migrationBuilder.DropColumn(
                name: "LibraryMemberId1",
                table: "LibraryFines");

            migrationBuilder.DropColumn(
                name: "PublisherId1",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "BookId1",
                table: "BookReviews");

            migrationBuilder.DropColumn(
                name: "BookId1",
                table: "BookReservations");

            migrationBuilder.DropColumn(
                name: "LibraryMemberId1",
                table: "BookReservations");

            migrationBuilder.DropColumn(
                name: "BookId1",
                table: "BookIssues");

            migrationBuilder.DropColumn(
                name: "LibraryMemberId1",
                table: "BookIssues");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BookIssueId1",
                table: "LibraryFines",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LibraryMemberId1",
                table: "LibraryFines",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PublisherId1",
                table: "Books",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BookId1",
                table: "BookReviews",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BookId1",
                table: "BookReservations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LibraryMemberId1",
                table: "BookReservations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BookId1",
                table: "BookIssues",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LibraryMemberId1",
                table: "BookIssues",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LibraryFines_BookIssueId1",
                table: "LibraryFines",
                column: "BookIssueId1");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryFines_LibraryMemberId1",
                table: "LibraryFines",
                column: "LibraryMemberId1");

            migrationBuilder.CreateIndex(
                name: "IX_Books_PublisherId1",
                table: "Books",
                column: "PublisherId1");

            migrationBuilder.CreateIndex(
                name: "IX_BookReviews_BookId1",
                table: "BookReviews",
                column: "BookId1");

            migrationBuilder.CreateIndex(
                name: "IX_BookReservations_BookId1",
                table: "BookReservations",
                column: "BookId1");

            migrationBuilder.CreateIndex(
                name: "IX_BookReservations_LibraryMemberId1",
                table: "BookReservations",
                column: "LibraryMemberId1");

            migrationBuilder.CreateIndex(
                name: "IX_BookIssues_BookId1",
                table: "BookIssues",
                column: "BookId1");

            migrationBuilder.CreateIndex(
                name: "IX_BookIssues_LibraryMemberId1",
                table: "BookIssues",
                column: "LibraryMemberId1");

            migrationBuilder.AddForeignKey(
                name: "FK_BookIssues_Books_BookId1",
                table: "BookIssues",
                column: "BookId1",
                principalTable: "Books",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookIssues_LibraryMembers_LibraryMemberId1",
                table: "BookIssues",
                column: "LibraryMemberId1",
                principalTable: "LibraryMembers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookReservations_Books_BookId1",
                table: "BookReservations",
                column: "BookId1",
                principalTable: "Books",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookReservations_LibraryMembers_LibraryMemberId1",
                table: "BookReservations",
                column: "LibraryMemberId1",
                principalTable: "LibraryMembers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookReviews_Books_BookId1",
                table: "BookReviews",
                column: "BookId1",
                principalTable: "Books",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Publishers_PublisherId1",
                table: "Books",
                column: "PublisherId1",
                principalTable: "Publishers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryFines_BookIssues_BookIssueId1",
                table: "LibraryFines",
                column: "BookIssueId1",
                principalTable: "BookIssues",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryFines_LibraryMembers_LibraryMemberId1",
                table: "LibraryFines",
                column: "LibraryMemberId1",
                principalTable: "LibraryMembers",
                principalColumn: "Id");
        }
    }
}
