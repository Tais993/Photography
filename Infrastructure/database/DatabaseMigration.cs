using EvolveDb;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure.database;

public class MigrationService
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<MigrationService> _logger;

    public MigrationService(NpgsqlDataSource dataSource, ILogger<MigrationService> logger)
    {
        _dataSource = dataSource;
        _logger = logger;
    }

    public void Migrate()
    {
        NpgsqlConnection connection = _dataSource.OpenConnection();


        string migrationPath = Path.Combine(AppContext.BaseDirectory, "database", "migrations");

        Evolve evolve = new Evolve(connection, (msg) => { _logger.LogDebug(msg); })
        {
            Locations = [migrationPath]
        };

        try
        {
            evolve.Migrate();
            _logger.LogInformation("Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database migration failed.");
            throw;
        }
    }
}