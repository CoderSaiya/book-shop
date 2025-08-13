using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookShop.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IndexOnUserAndProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_User_CreatedAt",
                table: "Users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_User_DateOfBirth",
                table: "Profiles",
                column: "DateOfBirth");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_CreatedAt",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_User_DateOfBirth",
                table: "Profiles");
        }
    }
}
