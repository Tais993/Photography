using Npgsql;
using Tests.Integration.Fixtures;

namespace Tests.Integration.Database;

public class DatabaseMigrationIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task Database_CanConnectAfterMigration()
    {
        object? result = await ExecuteScalarAsync("""
                                                  SELECT 1;
                                                  """);

        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public async Task DatabaseMigration_CreatesImageTable()
    {
        object? result = await ExecuteScalarAsync("""
                                                  SELECT EXISTS (
                                                      SELECT FROM information_schema.tables
                                                      WHERE table_schema = 'public'
                                                      AND table_name = 'image'
                                                  );
                                                  """);

        Assert.That(result, Is.EqualTo(true));
    }

    [Test]
    public async Task DatabaseMigration_CreatesProjectMetadataTable()
    {
        object? result = await ExecuteScalarAsync("""
                                                  SELECT EXISTS (
                                                      SELECT FROM information_schema.tables
                                                      WHERE table_schema = 'public'
                                                      AND table_name = 'project_metadata'
                                                  );
                                                  """);

        Assert.That(result, Is.EqualTo(true));
    }

    [Test]
    public async Task DatabaseMigration_ProjectMetadataUsesMetadataKeyAsPrimaryKey()
    {
        object? result = await ExecuteScalarAsync("""
                                                  SELECT EXISTS (
                                                      SELECT 1
                                                      FROM information_schema.table_constraints tc
                                                      JOIN information_schema.key_column_usage kcu
                                                          ON tc.constraint_name = kcu.constraint_name
                                                          AND tc.table_schema = kcu.table_schema
                                                          AND tc.table_name = kcu.table_name
                                                      WHERE tc.table_schema = 'public'
                                                      AND tc.table_name = 'project_metadata'
                                                      AND tc.constraint_type = 'PRIMARY KEY'
                                                      AND kcu.column_name = 'metadata_key'
                                                  );
                                                  """);

        Assert.That(result, Is.EqualTo(true));
    }

    private async Task<object?> ExecuteScalarAsync(string commandText)
    {
        await using NpgsqlConnection connection = new(FixtureConnectionString);
        await connection.OpenAsync();

        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = commandText;

        return await command.ExecuteScalarAsync();
    }
}