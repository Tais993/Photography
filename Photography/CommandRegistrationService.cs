using Cli.Commands;
using Cli.Commands.ProjectCommand;
using Microsoft.Extensions.DependencyInjection;

namespace Cli;

public static class CommandRegistrationService
{
    public static void Register(IServiceCollection services)
    {
        services.AddTransient<SearchCommand>();
        services.AddTransient<InitialiseCommand>();
        services.AddTransient<TestCommand>();
        services.AddTransient<MetadataCommand>();
        services.AddTransient<ProjectCommand>();
        services.AddTransient<CopyCommand>();
    }
}