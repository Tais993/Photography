using Infrastructure.database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure;

public static class MigrationExtension
{
    extension(IHost host)
    {
        public void MigrateDatabase()
        {
            using IServiceScope scope = host.Services.CreateScope();

            MigrationService migrationService =
                scope.ServiceProvider.GetRequiredService<MigrationService>();

            migrationService.Migrate();
        }
    }
}