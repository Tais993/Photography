using Infrastructure.database;
using Infrastructure.database.repositories;
using Infrastructure.filesystem;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        string connectionString = config.GetConnectionString("Default");
        // ?? throw new InvalidOperationException("Missing connection string.");
        services.AddSingleton(new NpgsqlDataSourceBuilder(connectionString).Build());
        services.AddTransient<MigrationService>();
        services.AddTransient<RepositoryHelper>();

// Repositories
        services.AddTransient<ImageRepository>();
        services.AddTransient<ProjectRepository>();
        services.AddTransient<MetadataRepository>();
        services.AddTransient<ProjectMetadataRepository>();


        services.AddTransient<IFiles, Files>();
        return services;
    }
}