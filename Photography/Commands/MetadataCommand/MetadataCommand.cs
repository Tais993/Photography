using System.CommandLine;
using Application.interfaces.services;
using Domain.entities;
using static Cli.ExitCodes;

namespace Cli.Commands.MetadataCommand;

public class MetadataCommand : CommandBase
{
    private readonly IProjectMetadataService _projectMetadataService;
    private readonly IProjectService _projectService;

    public MetadataCommand(IProjectService projectService, IProjectMetadataService projectMetadataService)
    {
        _projectService = projectService;
        _projectMetadataService = projectMetadataService;
    }

    protected override string Name => "metadata";
    protected override string Description => "";
    private static List<string> Aliases => ["md", "meta"];

    protected override void Configure(Command command)
    {
        base.Configure(command);
        AddAliases(Aliases);

        command.Subcommands.Add(new CreateMetadataCommand(_projectMetadataService).Build());
        command.Subcommands.Add(new EditMetadataCommand(_projectMetadataService).Build());
        command.Subcommands.Add(new DeleteMetadataCommand(_projectMetadataService).Build());
    }

    public override int Run(ParseResult parseResult)
    {
        Project project = _projectService.ResolveProject(Directory.GetCurrentDirectory());

        Console.WriteLine(project);

        return Success;
    }
}