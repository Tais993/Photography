using System.CommandLine;

namespace PhotographyNET.cli;

public interface ICommand
{
    public int Run(ParseResult parseResult);
    public Command Build();
}