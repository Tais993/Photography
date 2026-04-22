using System.CommandLine;

namespace Cli.Commands;

public interface ICommand
{
    public int Run(ParseResult parseResult);
    public Command Build();
}