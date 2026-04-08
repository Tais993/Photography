using System.CommandLine;
using PhotographyNET.commands;

namespace PhotographyNET;

public class CommandRegistration
{
    public RootCommand Commands()
    {
        
        RootCommand rootCommand = new RootCommand("Photography projects tool!!");
        
        
        List<CommandBase> commands = new();

        commands.Add(new SearchCommand());


        

        commands.ForEach(commandBase =>
        {
            rootCommand.Add(commandBase.Build());
        });

        
        return rootCommand;
    }
}