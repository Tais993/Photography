using System.CommandLine;
using Application.interfaces.services.project;
using Microsoft.Extensions.Logging;
using static Cli.ExitCodes;

namespace Cli.Commands;

public class InitialiseCommand : CommandBase
{
    private readonly IProjectInitialisingService _projectInitialisingService;
    private readonly ILogger<InitialiseCommand> _logger;

    public InitialiseCommand(IProjectInitialisingService projectInitialisingService, ILogger<InitialiseCommand> logger)
    {
        _projectInitialisingService = projectInitialisingService;
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

        _projectInitialisingService.InitialiseFolder(consoleDirectory);

        return Success;
    }
}