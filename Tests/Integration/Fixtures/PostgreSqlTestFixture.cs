using Application;
using Infrastructure;
using Infrastructure.database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Tests.Integration.Fixtures;

public class PostgreSqlTestFixture
{
    private PostgreSqlContainer _container = null!;

    public IServiceProvider Services { get; private set; } = null!;
    public string ConnectionString => _container.GetConnectionString();

    public async Task StartAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:17")
            .WithDatabase("photography_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _container.StartAsync();

        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = ConnectionString,
                [Constants.ConfigImageViewerMode] = "disabled"
            })
            .Build();

        ServiceCollection services = new();

        services.AddSingleton(config);
        
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddLogic();
        services.AddInfrastructure(config);

        Services = services.BuildServiceProvider();

        using IServiceScope scope = Services.CreateScope();

        MigrationService migrationService =
            scope.ServiceProvider.GetRequiredService<MigrationService>();

        migrationService.Migrate();
    }

    public async Task ResetDatabaseAsync()
    {
        await using NpgsqlConnection connection = new(ConnectionString);
        await connection.OpenAsync();

        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = """
                              TRUNCATE TABLE
                                  image,
                                  metadata,
                                  project,
                                  project_metadata,
                                  selection_session,
                                  selection_session_image
                              RESTART IDENTITY CASCADE;
                              """;


        await command.ExecuteNonQueryAsync();
    }

    public async Task DisposeAsync()
    {
        if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }

        await _container.DisposeAsync();
    }
}