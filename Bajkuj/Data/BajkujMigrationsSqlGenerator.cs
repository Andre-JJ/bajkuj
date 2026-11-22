using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Update;
using Microting.EntityFrameworkCore.MySql.Infrastructure.Internal;
using Microting.EntityFrameworkCore.MySql.Migrations;

namespace Bajkuj.Data;

// The branch histories share Identity tables. Compile their EF operations to SQL
// and prepare them only when the preceding schema has not already created them.
#pragma warning disable EF1001
public sealed class BajkujMigrationsSqlGenerator(
    MigrationsSqlGeneratorDependencies dependencies, ICommandBatchPreparer commandBatchPreparer, IMySqlOptions options)
    : MySqlMigrationsSqlGenerator(dependencies, commandBatchPreparer, options)
{
    protected override void Generate(MigrationOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        if (operation is not ConditionalMigration conditional)
        {
            base.Generate(operation, model, builder);
            return;
        }

        var conditionalCommands = new MigrationCommandListBuilder(Dependencies);
        foreach (var step in conditional.Operations)
            base.Generate(step, model, conditionalCommands);

        foreach (var command in conditionalCommands.GetCommandList())
        {
            var sql = command.CommandText.TrimEnd().TrimEnd(';').Replace("'", "''");
            builder.AppendLine($"SET @bajkuj_migration_sql = IF({conditional.Condition}, '{sql}', 'DO 0');")
                .AppendLine("PREPARE bajkuj_migration_statement FROM @bajkuj_migration_sql;")
                .AppendLine("EXECUTE bajkuj_migration_statement;")
                .AppendLine("DEALLOCATE PREPARE bajkuj_migration_statement;")
                .EndCommand(command.TransactionSuppressed);
        }
    }
}
#pragma warning restore EF1001

internal sealed class ConditionalMigration(string condition, IReadOnlyList<MigrationOperation> operations) : MigrationOperation
{
    internal const string LegacyIdentityApplied =
        "EXISTS (SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261112100330_Initial')";

    internal string Condition { get; } = condition;
    internal IReadOnlyList<MigrationOperation> Operations { get; } = operations;

    internal static void Apply(MigrationBuilder migrationBuilder, string condition, Action<MigrationBuilder> configure)
    {
        var steps = new MigrationBuilder(migrationBuilder.ActiveProvider);
        configure(steps);
        migrationBuilder.Operations.Add(new ConditionalMigration(condition, steps.Operations));
    }
}
