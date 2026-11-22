using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Bajkuj.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalPlansAndBikeUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "CancelledOn",
                table: "BikeRentals",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Term",
                table: "BikeRentals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "BikeRentals",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "varchar(80)",
                maxLength: 80,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "varchar(80)",
                maxLength: 80,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Bicykle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Model = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Typ = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CenaZaMesiac = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CenaZaRok = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DatumPridania = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bicykle", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BikeUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BicykelId = table.Column<int>(type: "int", nullable: false),
                    Stav = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BikeUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BikeUnits_Bicykle_BicykelId",
                        column: x => x.BicykelId,
                        principalTable: "Bicykle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Bicykle",
                columns: new[] { "Id", "CenaZaMesiac", "CenaZaRok", "DatumPridania", "Model", "Typ" },
                values: new object[,]
                {
                    { 101, 8m, 85m, new DateTime(2026, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kellys Avenue 30", "Mestský" },
                    { 102, 12.50m, 133m, new DateTime(2026, 9, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "CTM Rambler 2.0", "Horský" },
                    { 103, 22m, 234m, new DateTime(2026, 10, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Cube Touring Hybrid ONE", "Elektrobicykel" },
                    { 104, 9m, 96m, new DateTime(2026, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "CTM Bora 2.0", "Mestský" },
                    { 105, 11m, 117m, new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Giant Escape 3", "Trekingový" },
                    { 106, 13m, 138m, new DateTime(2026, 10, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Merida Big Nine 20", "Horský" }
                });

            migrationBuilder.InsertData(
                table: "BikeUnits",
                columns: new[] { "Id", "BicykelId", "Stav" },
                values: new object[,]
                {
                    { 101, 101, "dostupny" },
                    { 102, 102, "prenajaty" },
                    { 103, 103, "v_servise" },
                    { 104, 104, "dostupny" },
                    { 105, 105, "dostupny" },
                    { 106, 106, "dostupny" },
                    { 1101, 101, "dostupny" },
                    { 1102, 102, "dostupny" },
                    { 1103, 103, "dostupny" },
                    { 1104, 104, "dostupny" },
                    { 1105, 105, "dostupny" },
                    { 1106, 106, "dostupny" },
                    { 2101, 101, "dostupny" },
                    { 2102, 102, "dostupny" },
                    { 2103, 103, "dostupny" },
                    { 2104, 104, "dostupny" },
                    { 2105, 105, "dostupny" },
                    { 2106, 106, "dostupny" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BikeRentals_BikeId",
                table: "BikeRentals",
                column: "BikeId");

            migrationBuilder.CreateIndex(
                name: "IX_BikeUnits_BicykelId_Stav",
                table: "BikeUnits",
                columns: new[] { "BicykelId", "Stav" });

            migrationBuilder.AddForeignKey(
                name: "FK_BikeRentals_BikeUnits_BikeId",
                table: "BikeRentals",
                column: "BikeId",
                principalTable: "BikeUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BikeRentals_BikeUnits_BikeId",
                table: "BikeRentals");

            migrationBuilder.DropTable(
                name: "BikeUnits");

            migrationBuilder.DropTable(
                name: "Bicykle");

            migrationBuilder.DropIndex(
                name: "IX_BikeRentals_BikeId",
                table: "BikeRentals");

            migrationBuilder.DropColumn(
                name: "CancelledOn",
                table: "BikeRentals");

            migrationBuilder.DropColumn(
                name: "Term",
                table: "BikeRentals");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "BikeRentals");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AspNetUsers");
        }
    }
}
