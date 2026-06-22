using System.CommandLine;
using Cli.Commands;
using Cli.Commands.ProjectCommand;
using Microsoft.Extensions.DependencyInjection;

namespace Cli;

public static class CommandFactory
{
    public static RootCommand Commands(IServiceProvider provider)
    {
        RootCommand rootCommand = new RootCommand("Photography projects tool!!")
        {
            provider.GetRequiredService<SearchCommand>().Build(),
            provider.GetRequiredService<InitialiseCommand>().Build(),
            provider.GetRequiredService<TestCommand>().Build(),
            provider.GetRequiredService<MetadataCommand>().Build(),
            provider.GetRequiredService<ProjectCommand>().Build(),
            provider.GetRequiredService<CopyCommand>().Build(),
            provider.GetRequiredService<SelectCommand>().Build(),
            provider.GetRequiredService<ApplicationResetCommand>().Build(),
        };

        return rootCommand;
    }
}