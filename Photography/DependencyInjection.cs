using Microsoft.Extensions.DependencyInjection;

namespace Cli;

public static class DependencyInjection
{
    public static IServiceCollection AddCli(this IServiceCollection services)
    {
        CommandRegistrationService.Register(services);
        return services;
    }
}