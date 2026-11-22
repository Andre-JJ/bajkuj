using Microsoft.EntityFrameworkCore.Migrations;

namespace Bajkuj.Data.Migrations;

public partial class ReconcileLegacyAccountNames : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ConditionalMigration.Apply(migrationBuilder,
            """
            EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'Meno')
            """,
            steps => steps.Sql(
                """
                UPDATE `AspNetUsers`
                SET `FirstName` = COALESCE(`FirstName`, LEFT(NULLIF(TRIM(`Meno`), ''), 80)),
                    `LastName` = COALESCE(`LastName`, LEFT(NULLIF(TRIM(`Priezvisko`), ''), 80))
                """));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Keep names that may have been updated since the migration.
    }
}
