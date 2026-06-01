using Infrastructure.database;
using Infrastructure.database.repositories;
using Infrastructure.filesystem;
using Infrastructure.irfanview;
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
        services.AddTransient<IImageRepository, ImageRepository>();
        services.AddTransient<IProjectRepository, ProjectRepository>();
        services.AddTransient<IMetadataRepository, MetadataRepository>();
        services.AddTransient<IProjectMetadataRepository, ProjectMetadataRepository>();
        services.AddTransient<ISelectionRepository, SelectionRepository>();


        services.AddTransient<IFiles, Files>();

        services.AddTransient<IIrfanViewRepository, IrfanViewRepository>();
        return services;
    }
}