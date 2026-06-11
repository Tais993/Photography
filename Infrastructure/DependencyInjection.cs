using Application.interfaces;
using Infrastructure.database;
using Infrastructure.database.repositories;
using Infrastructure.filesystem;
using Infrastructure.images;
using Infrastructure.irfanview;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Infrastructure;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration config)
        {
            string connectionString = config.GetConnectionString("Default");
            // ?? throw new InvalidOperationException("Missing connection string.");
            services.AddSingleton(new NpgsqlDataSourceBuilder(connectionString).Build());
            services.AddTransient<MigrationService>();
            services.AddScoped<RepositoryHelper>();

            // Repositories
            services.AddScoped<IImageRepository, ImageRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IMetadataRepository, MetadataRepository>();
            services.AddScoped<IProjectMetadataRepository, ProjectMetadataRepository>();
            services.AddScoped<ISelectionRepository, SelectionRepository>();


            services.AddScoped<IFiles, Files>();

            services.AddScoped<IIrfanViewRepository, IrfanViewRepository>();

            services.AddScoped<IThumbnailGenerator, MagickThumbnailGenerator>();
            return services;
        }
    }
}