using System.CommandLine;
using Application.services.interfaces;

namespace Cli.Commands.ProjectCommand;

public class ProjectCommand : CommandBase
{
    private readonly IProjectMetadataService _projectMetadataService;
    private readonly IProjectService _projectService;

    public ProjectCommand(IProjectService projectService, IProjectMetadataService projectMetadataService)
    {
        _projectService = projectService;
        _projectMetadataService = projectMetadataService;
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
        command.Subcommands.Add(new AddMetadataCommand(_projectService, _projectMetadataService).Build());
        command.Subcommands.Add(new RemoveMetadataCommand(_projectService, _projectMetadataService).Build());
        // command.Subcommands.Add(verifyCommand);
    }

    public override int Run(ParseResult parseResult)
    {
        // Console.WriteLine(project);

        return 0;
    }
}