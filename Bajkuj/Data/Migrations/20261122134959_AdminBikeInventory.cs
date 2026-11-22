using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bajkuj.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdminBikeInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BikeRentals_BikeUnits_BikeId",
                table: "BikeRentals");

            migrationBuilder.DropIndex(
                name: "IX_BikeRentals_BikeId",
                table: "BikeRentals");

            migrationBuilder.AddColumn<int>(
                name: "BikeUnitId",
                table: "BikeRentals",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasCustomPhoto",
                table: "Bicykle",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "BikePhotos",
                columns: table => new
                {
                    BicykelId = table.Column<int>(type: "int", nullable: false),
                    ContentType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<byte[]>(type: "mediumblob", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BikePhotos", x => x.BicykelId);
                    table.ForeignKey(
                        name: "FK_BikePhotos_Bicykle_BicykelId",
                        column: x => x.BicykelId,
                        principalTable: "Bicykle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("UPDATE `BikeRentals` SET `BikeUnitId` = `BikeId`");

            migrationBuilder.CreateIndex(
                name: "IX_BikeRentals_BikeUnitId",
                table: "BikeRentals",
                column: "BikeUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_BikeRentals_BikeUnits_BikeUnitId",
                table: "BikeRentals",
                column: "BikeUnitId",
                principalTable: "BikeUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BikeRentals_BikeUnits_BikeUnitId",
                table: "BikeRentals");

            migrationBuilder.DropTable(
                name: "BikePhotos");

            migrationBuilder.DropIndex(
                name: "IX_BikeRentals_BikeUnitId",
                table: "BikeRentals");

            migrationBuilder.DropColumn(
                name: "BikeUnitId",
                table: "BikeRentals");

            migrationBuilder.DropColumn(
                name: "HasCustomPhoto",
                table: "Bicykle");

            migrationBuilder.CreateIndex(
                name: "IX_BikeRentals_BikeId",
                table: "BikeRentals",
                column: "BikeId");

            migrationBuilder.AddForeignKey(
                name: "FK_BikeRentals_BikeUnits_BikeId",
                table: "BikeRentals",
                column: "BikeId",
                principalTable: "BikeUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
