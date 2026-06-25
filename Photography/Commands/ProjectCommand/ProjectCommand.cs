using System.CommandLine;
using Application.interfaces.services.metadata;
using Application.interfaces.services.project;
using static Cli.ExitCodes;

namespace Cli.Commands.ProjectCommand;

public class ProjectCommand : CommandBase
{
    private readonly IMetadataService _metadataService;
    private readonly IProjectMetadataService _projectMetadataService;
    private readonly IProjectResolverService _projectResolverService;

    public ProjectCommand(IProjectMetadataService projectMetadataService, IProjectResolverService projectResolverService, IMetadataService metadataService)
    {
        _projectMetadataService = projectMetadataService;
        _projectResolverService = projectResolverService;
        _metadataService = metadataService;
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
        command.Subcommands.Add(new AddMetadataCommand(_projectResolverService, _projectMetadataService, _metadataService).Build());
        command.Subcommands.Add(new RemoveMetadataCommand(_projectResolverService, _projectMetadataService, _metadataService).Build());
        // command.Subcommands.Add(verifyCommand);
    }

    public override int Run(ParseResult parseResult)
    {
        // Console.WriteLine(project);

        return Success;
    }
}