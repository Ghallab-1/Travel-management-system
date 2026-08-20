using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelManagement.Infrastructure.TravelManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleLevelAndApprovalChain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentApprovalLevel",
                table: "TravelRequests",
                type: "integer",
                nullable: false,
                defaultValue: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentApprovalLevel",
                table: "TravelRequests");
        }
    }
}
