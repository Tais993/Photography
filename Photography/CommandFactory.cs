using System.CommandLine;
using Cli.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Cli;

public class CommandFactory
{

    public static RootCommand Commands(IServiceProvider provider)
    {
        RootCommand rootCommand = new RootCommand("Photography projects tool!!")
        {
            provider.GetRequiredService<SearchCommand>().Build(),
            provider.GetRequiredService<InitialiseCommand>().Build(),
            provider.GetRequiredService<TestCommand>().Build()
        };

        return rootCommand;
    }
}