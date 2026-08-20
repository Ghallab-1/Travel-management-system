using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelManagement.Infrastructure.TravelManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Roles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "Roles");
        }
    }
}
