using System.CommandLine;

namespace Cli.Commands;

public abstract class CommandBase : ICommand
{
    protected Command Command = null!;

    protected abstract string Name { get; }
    protected abstract string Description { get; }

    public abstract int Run(ParseResult parseResult);


    public Command Build()
    {
        var command = new Command(Name, Description);
        Command = command;
        command.SetAction(Run);


        Configure(command);
        return command;
    }

    public void AddAliases(List<string> aliases)
    {
        foreach (var alias in aliases)
        {
            Command.Aliases.Add(alias);
        }
    }

    public static int InvalidInput(string warningText)
    {
        Console.WriteLine(warningText);
        return ExitCodes.InvalidInput;
    }

    /// <summary>
    ///     This method can be used for configuring anything outside of the name, description and aliases.
    ///     While the command is getting build this method gets called with the command.
    /// </summary>
    /// <param name="command"></param>
    protected virtual void Configure(Command command)
    {
    }
}