using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Bajkuj.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExpandInventoryAndAddCheckout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckoutSessionId",
                table: "BikeRentals",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "PaymentPending",
                table: "BikeRentals",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 1102,
                column: "Stav",
                value: "prenajaty");

            migrationBuilder.UpdateData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 1103,
                column: "Stav",
                value: "v_servise");

            migrationBuilder.UpdateData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 2102,
                column: "Stav",
                value: "prenajaty");

            migrationBuilder.UpdateData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 2103,
                column: "Stav",
                value: "v_servise");

            migrationBuilder.InsertData(
                table: "BikeUnits",
                columns: new[] { "Id", "BicykelId", "Stav" },
                values: new object[,]
                {
                    { 10103, 101, "dostupny" },
                    { 10104, 101, "dostupny" },
                    { 10105, 101, "dostupny" },
                    { 10106, 101, "dostupny" },
                    { 10107, 101, "dostupny" },
                    { 10108, 101, "dostupny" },
                    { 10109, 101, "dostupny" },
                    { 10203, 102, "prenajaty" },
                    { 10204, 102, "prenajaty" },
                    { 10303, 103, "v_servise" },
                    { 10403, 104, "dostupny" },
                    { 10404, 104, "dostupny" },
                    { 10405, 104, "dostupny" },
                    { 10406, 104, "dostupny" },
                    { 10503, 105, "dostupny" },
                    { 10504, 105, "dostupny" },
                    { 10505, 105, "dostupny" },
                    { 10506, 105, "dostupny" },
                    { 10507, 105, "dostupny" },
                    { 10508, 105, "dostupny" },
                    { 10509, 105, "dostupny" },
                    { 10510, 105, "dostupny" },
                    { 10511, 105, "dostupny" },
                    { 10603, 106, "dostupny" },
                    { 10604, 106, "dostupny" },
                    { 10605, 106, "dostupny" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BikeRentals_CheckoutSessionId",
                table: "BikeRentals",
                column: "CheckoutSessionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BikeRentals_CheckoutSessionId",
                table: "BikeRentals");

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10103);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10104);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10105);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10106);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10107);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10108);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10109);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10203);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10204);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10303);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10403);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10404);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10405);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10406);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10503);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10504);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10505);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10506);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10507);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10508);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10509);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10510);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10511);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10603);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10604);

            migrationBuilder.DeleteData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 10605);

            migrationBuilder.DropColumn(
                name: "CheckoutSessionId",
                table: "BikeRentals");

            migrationBuilder.DropColumn(
                name: "PaymentPending",
                table: "BikeRentals");

            migrationBuilder.UpdateData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 1102,
                column: "Stav",
                value: "dostupny");

            migrationBuilder.UpdateData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 1103,
                column: "Stav",
                value: "dostupny");

            migrationBuilder.UpdateData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 2102,
                column: "Stav",
                value: "dostupny");

            migrationBuilder.UpdateData(
                table: "BikeUnits",
                keyColumn: "Id",
                keyValue: 2103,
                column: "Stav",
                value: "dostupny");
        }
    }
}
