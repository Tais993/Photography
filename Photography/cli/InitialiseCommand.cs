using System.CommandLine;
using PhotographyNET.services.interfaces;

namespace PhotographyNET.cli;

public class InitialiseCommand : CommandBase
{

    private readonly IProjectResolver _projectResolver;
    private readonly ILogger<InitialiseCommand> _logger;

    public InitialiseCommand(IProjectResolver projectResolver, ILogger<InitialiseCommand> logger)
    {
        _projectResolver = projectResolver;
        _logger = logger;
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
    
    

    public override int Run(ParseResult parseResult)
    {
        string consoleDirectory = Directory.GetCurrentDirectory();
        string[] subdirectories = Directory.GetDirectories(consoleDirectory);
        
        foreach (string subdirectory in subdirectories)
        {
            _projectResolver.initialiseExistingFolder(subdirectory);
        }
        
        return 0;
    }
}