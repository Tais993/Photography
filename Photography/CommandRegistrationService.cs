using Cli.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Cli;

public class CommandRegistrationService
{
    public static void Register(IServiceCollection services)
    {
        services.AddTransient<SearchCommand>();
        services.AddTransient<InitialiseCommand>();
        services.AddTransient<TestCommand>();
    }
}