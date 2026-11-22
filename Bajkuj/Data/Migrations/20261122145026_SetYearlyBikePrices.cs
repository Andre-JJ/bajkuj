using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bajkuj.Data.Migrations
{
    /// <inheritdoc />
    public partial class SetYearlyBikePrices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Bicykle",
                keyColumn: "Id",
                keyValue: 101,
                column: "CenaZaRok",
                value: 27m);

            migrationBuilder.UpdateData(
                table: "Bicykle",
                keyColumn: "Id",
                keyValue: 102,
                column: "CenaZaRok",
                value: 31m);

            migrationBuilder.UpdateData(
                table: "Bicykle",
                keyColumn: "Id",
                keyValue: 103,
                column: "CenaZaRok",
                value: 36m);

            migrationBuilder.UpdateData(
                table: "Bicykle",
                keyColumn: "Id",
                keyValue: 104,
                column: "CenaZaRok",
                value: 29m);

            migrationBuilder.UpdateData(
                table: "Bicykle",
                keyColumn: "Id",
                keyValue: 105,
                column: "CenaZaRok",
                value: 32m);

            migrationBuilder.UpdateData(
                table: "Bicykle",
                keyColumn: "Id",
                keyValue: 106,
                column: "CenaZaRok",
                value: 33m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Bicykle",
                keyColumn: "Id",
                keyValue: 101,
                column: "CenaZaRok",
                value: 85m);

            migrationBuilder.UpdateData(
                table: "Bicykle",
                keyColumn: "Id",
                keyValue: 102,
                column: "CenaZaRok",
                value: 133m);

            migrationBuilder.UpdateData(
                table: "Bicykle",
                keyColumn: "Id",
                keyValue: 103,
                column: "CenaZaRok",
                value: 234m);

            migrationBuilder.UpdateData(
                table: "Bicykle",
                keyColumn: "Id",
                keyValue: 104,
                column: "CenaZaRok",
                value: 96m);

            migrationBuilder.UpdateData(
                table: "Bicykle",
                keyColumn: "Id",
                keyValue: 105,
                column: "CenaZaRok",
                value: 117m);

            migrationBuilder.UpdateData(
                table: "Bicykle",
                keyColumn: "Id",
                keyValue: 106,
                column: "CenaZaRok",
                value: 138m);
        }
    }
}
