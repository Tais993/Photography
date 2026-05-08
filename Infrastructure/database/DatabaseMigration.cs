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
        var connection = _dataSource.OpenConnection();


        var migrationPath = Path.Combine(AppContext.BaseDirectory, "database", "migrations");
        _logger.LogInformation("Migration path: {MigrationPath}", migrationPath);

        var evolve = new Evolve(connection, msg =>
        {
            _logger.LogInformation(msg);
        })
        {
            Locations = new[] { migrationPath },
        };

        evolve.Migrate();
    }
}