using System.CommandLine;
using PhotographyNET.services.interfaces;

namespace PhotographyNET.commands;

public class InitialiseCommand : CommandBase
{

    private IProjectResolver projectResolver;

    public InitialiseCommand(IProjectResolver projectResolver)
    {
        this.projectResolver = projectResolver;
    }
    
    
    
    protected override string Name => "initialise";
    protected override string Description => "Initialises all project folders, and recognizes the current structure";
    

    private readonly Option<string> _mainFolder = new("--main-folder")
    {

        Description = "Use exact matching only"
    };

    protected override void Configure(Command command)
    {
        base.Configure(command);
        
        command.Options.Add(_mainFolder);
    }
    
    

    protected override int Run(ParseResult parseResult)
    {
        string consoleDirectory = Directory.GetCurrentDirectory();
        string[] subdirectories = Directory.GetDirectories(consoleDirectory);
        
        foreach (string subdirectory in subdirectories)
        {
            projectResolver.initialiseExistingFolder(subdirectory);
        }
        
        return 0;
    }
}