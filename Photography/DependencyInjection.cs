using Microsoft.Extensions.DependencyInjection;

namespace Cli;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCli()
        {
            CommandRegistrationService.Register(services);
            return services;
        }
    }
}