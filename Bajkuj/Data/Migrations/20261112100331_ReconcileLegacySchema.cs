using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bajkuj.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261112100331_ReconcileLegacySchema")]
public sealed class ReconcileLegacySchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Databases created from main already have a daily-rate catalog with attached records.
        // Preserve that catalog before the monthly/yearly migration creates its own Bicykle table.
        ConditionalMigration.Apply(migrationBuilder,
            """
            EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Bicykle' AND COLUMN_NAME = 'CenaZaDen')
            """,
            steps => steps.Sql("RENAME TABLE `Bicykle` TO `LegacyBicykle`", suppressTransaction: true));

        ConditionalMigration.Apply(migrationBuilder,
            """
            EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'Meno')
            """,
            steps => steps.Sql(
                "ALTER TABLE `AspNetUsers` MODIFY `Meno` longtext NULL, MODIFY `Priezvisko` longtext NULL",
                suppressTransaction: true));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // The preceding migration in the combined history also uses LegacyBicykle.
    }
}
