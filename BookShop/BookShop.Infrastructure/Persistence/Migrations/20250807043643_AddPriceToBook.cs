using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookShop.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceToBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SubscriberNumber",
                table: "Profiles",
                newName: "Phone_SubscriberNumber");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Profiles",
                newName: "Name_LastName");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Profiles",
                newName: "Name_FirstName");

            migrationBuilder.RenameColumn(
                name: "CountryCode",
                table: "Profiles",
                newName: "Phone_CountryCode");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Books",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Books");

            migrationBuilder.RenameColumn(
                name: "Phone_SubscriberNumber",
                table: "Profiles",
                newName: "SubscriberNumber");

            migrationBuilder.RenameColumn(
                name: "Phone_CountryCode",
                table: "Profiles",
                newName: "CountryCode");

            migrationBuilder.RenameColumn(
                name: "Name_LastName",
                table: "Profiles",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "Name_FirstName",
                table: "Profiles",
                newName: "FirstName");
        }
    }
}
