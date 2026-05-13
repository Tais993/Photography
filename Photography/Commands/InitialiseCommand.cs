using System.CommandLine;
using Application.services.interfaces;
using Microsoft.Extensions.Logging;

namespace Cli.Commands;

public class InitialiseCommand : CommandBase
{
    private readonly IProjectService _projectService;
    private readonly ILogger<InitialiseCommand> _logger;

    public InitialiseCommand(IProjectService projectService, ILogger<InitialiseCommand> logger)
    {
        _projectService = projectService;
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
            _projectService.InitialiseExistingFolder(subdirectory);
        }

        return 0;
    }
}