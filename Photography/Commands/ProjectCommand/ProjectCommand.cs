using System.CommandLine;
using Application.interfaces.services;
using Application.interfaces.services.metadata;
using Application.interfaces.services.project;
using Application.interfaces.website;
using Microsoft.Extensions.Logging;
using static Cli.ExitCodes;

namespace Cli.Commands.ProjectCommand;

public class ProjectCommand : CommandBase
{
    private readonly IProjectService _projectService;
    private readonly IProjectImportService _projectImportService;
    private readonly IProjectIndexService _projectIndexService;
    private readonly ICameraDriveService _cameraDriveService;

    private readonly IMetadataService _metadataService;
    private readonly IProjectMetadataService _projectMetadataService;
    private readonly IProjectResolverService _projectResolverService;

    private readonly ILogger<CreateProjectCommand> _loggerCreateProjectCommand;
    private readonly ILogger<ImportCommand> _loggerImportCommand;

    public ProjectCommand(IProjectMetadataService projectMetadataService, IProjectResolverService projectResolverService, IMetadataService metadataService, 
        IProjectService projectService, IProjectImportService projectImportService, IProjectIndexService projectIndexService, ICameraDriveService cameraDriveService,
        ILogger<CreateProjectCommand> loggerCreateProjectCommand, ILogger<ImportCommand> loggerImportCommand)
    {
        _projectMetadataService = projectMetadataService;
        _projectResolverService = projectResolverService;
        _metadataService = metadataService;
        _projectService = projectService;
        _projectImportService = projectImportService;
        _projectIndexService = projectIndexService;
        _cameraDriveService = cameraDriveService;
        _loggerCreateProjectCommand = loggerCreateProjectCommand;
        _loggerImportCommand = loggerImportCommand;
    }


    protected override string Name => "project";
    protected override string Description => "";


    protected override void Configure(Command command)
    {
        base.Configure(command);

        // Command createCommand = new Command("create", "Delete metadata");
        // Command editCommand = new Command("edit", "Delete metadata");
        // Command deleteCommand = new Command("delete", "Delete metadata");
        // Command deleteMetadataCommand = new Command("delete-metadata", "Delete metadata");
        // Command verifyCommand = new Command("verify", "Delete metadata");
        //
        //
        // command.Subcommands.Add(createCommand);
        // command.Subcommands.Add(editCommand);
        // command.Subcommands.Add(deleteCommand);
        command.Subcommands.Add(new AddMetadataCommand(_projectResolverService, _projectMetadataService, _metadataService)
            .Build());
        command.Subcommands.Add(new RemoveMetadataCommand(_projectResolverService, _projectMetadataService, _metadataService)
            .Build());
        command.Subcommands.Add(new CreateProjectCommand(_projectService, _projectImportService, _projectIndexService,
            _cameraDriveService, _loggerCreateProjectCommand).Build());
        command.Subcommands.Add(new ImportCommand(_projectImportService, _projectResolverService, _projectService, _cameraDriveService, _loggerImportCommand).Build());
        // command.Subcommands.Add(verifyCommand);
    }

    public override int Run(ParseResult parseResult)
    {
        // Console.WriteLine(project);

        return Success;
    }
}