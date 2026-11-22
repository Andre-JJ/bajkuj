using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bajkuj.Data.Migrations
{
    /// <inheritdoc />
    public partial class LabelTestRentals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTestRental",
                table: "BikeRentals",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTestRental",
                table: "BikeRentals");
        }
    }
}
