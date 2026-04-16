using EvolveDb;
using Npgsql;

namespace PhotographyNET.database;

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

        var evolve = new Evolve(connection, msg =>
        {
            _logger.LogInformation(msg);
        })
        {
            Locations = new[] { "database/migrations" },
        };

        evolve.Migrate();
    }
}