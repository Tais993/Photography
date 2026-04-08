using System.CommandLine;

namespace PhotographyNET.commands;

public interface ICommand
{
    public List<Argument> Arguments();
    public List<Option> Options();
    public Command Command();


    public void Run(ParseResult parseResult);
}