using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelManagement.Infrastructure.TravelManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCoordinatorDocumentsPerDiem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoordinatorNotes",
                table: "TravelRequests",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedBudgetSetById",
                table: "TravelRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedBudgetSetDate",
                table: "TravelRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PerDiemAmount",
                table: "TravelRequests",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PerDiemApprovedById",
                table: "TravelRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerDiemComments",
                table: "TravelRequests",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PerDiemDecisionDate",
                table: "TravelRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerDiemStatus",
                table: "TravelRequests",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Not Submitted");

            migrationBuilder.AddColumn<string>(
                name: "RequiredDocumentFileBase64",
                table: "TravelRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequiredDocumentFileContentType",
                table: "TravelRequests",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequiredDocumentFileName",
                table: "TravelRequests",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequiredDocumentNotes",
                table: "TravelRequests",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TravelRequests_EstimatedBudgetSetById",
                table: "TravelRequests",
                column: "EstimatedBudgetSetById");

            migrationBuilder.CreateIndex(
                name: "IX_TravelRequests_PerDiemApprovedById",
                table: "TravelRequests",
                column: "PerDiemApprovedById");

            migrationBuilder.AddForeignKey(
                name: "FK_TravelRequests_Users_EstimatedBudgetSetById",
                table: "TravelRequests",
                column: "EstimatedBudgetSetById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TravelRequests_Users_PerDiemApprovedById",
                table: "TravelRequests",
                column: "PerDiemApprovedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TravelRequests_Users_EstimatedBudgetSetById",
                table: "TravelRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_TravelRequests_Users_PerDiemApprovedById",
                table: "TravelRequests");

            migrationBuilder.DropIndex(
                name: "IX_TravelRequests_EstimatedBudgetSetById",
                table: "TravelRequests");

            migrationBuilder.DropIndex(
                name: "IX_TravelRequests_PerDiemApprovedById",
                table: "TravelRequests");

            migrationBuilder.DropColumn(
                name: "CoordinatorNotes",
                table: "TravelRequests");

            migrationBuilder.DropColumn(
                name: "EstimatedBudgetSetById",
                table: "TravelRequests");

            migrationBuilder.DropColumn(
                name: "EstimatedBudgetSetDate",
                table: "TravelRequests");

            migrationBuilder.DropColumn(
                name: "PerDiemAmount",
                table: "TravelRequests");

            migrationBuilder.DropColumn(
                name: "PerDiemApprovedById",
                table: "TravelRequests");

            migrationBuilder.DropColumn(
                name: "PerDiemComments",
                table: "TravelRequests");

            migrationBuilder.DropColumn(
                name: "PerDiemDecisionDate",
                table: "TravelRequests");

            migrationBuilder.DropColumn(
                name: "PerDiemStatus",
                table: "TravelRequests");

            migrationBuilder.DropColumn(
                name: "RequiredDocumentFileBase64",
                table: "TravelRequests");

            migrationBuilder.DropColumn(
                name: "RequiredDocumentFileContentType",
                table: "TravelRequests");

            migrationBuilder.DropColumn(
                name: "RequiredDocumentFileName",
                table: "TravelRequests");

            migrationBuilder.DropColumn(
                name: "RequiredDocumentNotes",
                table: "TravelRequests");
        }
    }
}
