using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bajkuj.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The combined history creates Identity in CreateIdentitySchema.
            // Keep the original migration ID so existing main databases retain their history.
            migrationBuilder.CreateTable(
                name: "LegacyBicykle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Model = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Typ = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Stav = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CenaZaDen = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DatumPridania = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegacyBicykle", x => x.Id);
                    table.CheckConstraint("CK_Bicykel_Cena", "CenaZaDen >= 0");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Prenajmy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BicykelId = table.Column<int>(type: "int", nullable: false),
                    DatumOd = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DatumDo = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CelkovaCena = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    Stav = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prenajmy", x => x.Id);
                    table.CheckConstraint("CK_Prenajom_Datum", "DatumDo IS NULL OR DatumDo > DatumOd");
                    table.ForeignKey(
                        name: "FK_Prenajmy_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Prenajmy_Bicykle_BicykelId",
                        column: x => x.BicykelId,
                        principalTable: "LegacyBicykle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Servisy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BicykelId = table.Column<int>(type: "int", nullable: false),
                    DatumServisu = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Popis = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Naklady = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    Stav = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servisy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Servisy_Bicykle_BicykelId",
                        column: x => x.BicykelId,
                        principalTable: "LegacyBicykle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Platby",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PrenajomId = table.Column<int>(type: "int", nullable: false),
                    Suma = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DatumPlatby = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SposobPlatby = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Stav = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platby", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Platby_Prenajmy_PrenajomId",
                        column: x => x.PrenajomId,
                        principalTable: "Prenajmy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Zmluvy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PrenajomId = table.Column<int>(type: "int", nullable: false),
                    CisloZmluvy = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DatumVytvorenia = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DatumPodpisu = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Stav = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DokumentUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zmluvy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Zmluvy_Prenajmy_PrenajomId",
                        column: x => x.PrenajomId,
                        principalTable: "Prenajmy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Platby_PrenajomId",
                table: "Platby",
                column: "PrenajomId");

            migrationBuilder.CreateIndex(
                name: "IX_Prenajmy_BicykelId",
                table: "Prenajmy",
                column: "BicykelId");

            migrationBuilder.CreateIndex(
                name: "IX_Prenajmy_UserId",
                table: "Prenajmy",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Servisy_BicykelId",
                table: "Servisy",
                column: "BicykelId");

            migrationBuilder.CreateIndex(
                name: "IX_Zmluvy_CisloZmluvy",
                table: "Zmluvy",
                column: "CisloZmluvy",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Zmluvy_PrenajomId",
                table: "Zmluvy",
                column: "PrenajomId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Platby");

            migrationBuilder.DropTable(
                name: "Servisy");

            migrationBuilder.DropTable(
                name: "Zmluvy");

            migrationBuilder.DropTable(
                name: "Prenajmy");

            migrationBuilder.DropTable(
                name: "LegacyBicykle");
        }
    }
}
