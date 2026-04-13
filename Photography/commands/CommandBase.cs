using System.CommandLine;

namespace PhotographyNET.commands;

public abstract class CommandBase
{
    protected abstract string Name { get; }
    protected abstract string Description { get; }

    protected virtual IEnumerable<string> Aliases => [];
    
    /// <summary>
    /// This method can be used for configuring anything outside of the name, description and aliases.
    /// While the command is getting build this method gets called with the command.
    /// </summary>
    /// <param name="command"></param>
    protected virtual void Configure(Command command) { }

    public abstract int Run(ParseResult parseResult);

    
    public virtual Command Build()
    {
        Command command = new Command(Name, Description);

        foreach (string alias in Aliases)
        {
            command.Aliases.Add(alias);
        }
        
        Configure(command);

        command.SetAction(Run);

        return command;
    }
}