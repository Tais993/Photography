using Application.interfaces.infrastructure.services;
using Infrastructure.database;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Tests.Integration.Fixtures;

namespace Tests.Integration.Infrastructure;

public class DatabaseResetServiceIntegrationTests : IntegrationTestBase
{
    private static readonly string[] ApplicationTables =
    [
        "selection_session_image",
        "selection_session",
        "project_metadata",
        "metadata",
        "image",
        "project",
        "changelog"
    ];

    [Test]
    public void DropDatabase_DropsAllApplicationTables()
    {
        using IServiceScope scope = CreateScope();
        IDatabaseResetService databaseResetService =
            scope.ServiceProvider.GetRequiredService<IDatabaseResetService>();
        MigrationService migrationService =
            scope.ServiceProvider.GetRequiredService<MigrationService>();
        NpgsqlDataSource dataSource =
            scope.ServiceProvider.GetRequiredService<NpgsqlDataSource>();

        try
        {
            // Setup & Asserts
            AssertApplicationTablesExist(dataSource);

            // Execution
            databaseResetService.DropDatabase();

            // Asserts
            AssertApplicationTablesDoNotExist(dataSource);
        }
        finally
        {
            migrationService.Migrate();
        }
    }

    [Test]
    public void DropDatabase_WhenTablesAlreadyDropped_DoesNotThrow()
    {
        using IServiceScope scope = CreateScope();
        IDatabaseResetService databaseResetService =
            scope.ServiceProvider.GetRequiredService<IDatabaseResetService>();
        MigrationService migrationService =
            scope.ServiceProvider.GetRequiredService<MigrationService>();
        NpgsqlDataSource dataSource =
            scope.ServiceProvider.GetRequiredService<NpgsqlDataSource>();

        try
        {
            // Setup
            databaseResetService.DropDatabase();

            // Execution & Asserts
            Assert.DoesNotThrow(() => databaseResetService.DropDatabase());

            // Asserts
            AssertApplicationTablesDoNotExist(dataSource);
        }
        finally
        {
            migrationService.Migrate();
        }
    }

    [Test]
    public void DropDatabase_AllowsMigrationsToRunAgainAfterDrop()
    {
        using IServiceScope scope = CreateScope();
        IDatabaseResetService databaseResetService =
            scope.ServiceProvider.GetRequiredService<IDatabaseResetService>();
        MigrationService migrationService =
            scope.ServiceProvider.GetRequiredService<MigrationService>();
        NpgsqlDataSource dataSource =
            scope.ServiceProvider.GetRequiredService<NpgsqlDataSource>();

        // Execution
        databaseResetService.DropDatabase();
        migrationService.Migrate();

        // Asserts
        AssertApplicationTablesExist(dataSource);
    }

    
    private static void AssertApplicationTablesExist(NpgsqlDataSource dataSource)
    {
        using (Assert.EnterMultipleScope())
        {
            foreach (string tableName in ApplicationTables)
            {
                Assert.That(
                    TableExists(dataSource, tableName),
                    Is.True,
                    $"Table should exist: {tableName}");
            }
        }
    }

    private static void AssertApplicationTablesDoNotExist(NpgsqlDataSource dataSource)
    {
        using (Assert.EnterMultipleScope())
        {
            foreach (string tableName in ApplicationTables)
            {
                Assert.That(
                    TableExists(dataSource, tableName),
                    Is.False,
                    $"Table should not exist: {tableName}");
            }
        }
    }

    private static bool TableExists(NpgsqlDataSource dataSource, string tableName)
    {
        using NpgsqlCommand command = dataSource.CreateCommand("""
                                                               SELECT to_regclass(@TableName) IS NOT NULL
                                                               """);

        command.Parameters.AddWithValue("TableName", $"public.{tableName}");

        return (bool) command.ExecuteScalar()!;
    }
}