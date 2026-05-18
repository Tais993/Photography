using System.CommandLine;
using Cli.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Cli;

public static class CommandFactory
{
    public static RootCommand Commands(IServiceProvider provider)
    {
        var rootCommand = new RootCommand("Photography projects tool!!")
        {
            provider.GetRequiredService<SearchCommand>().Build(),
            provider.GetRequiredService<InitialiseCommand>().Build(),
            provider.GetRequiredService<TestCommand>().Build(),
            provider.GetRequiredService<MetadataCommand>().Build()
        };

        return rootCommand;
    }
}