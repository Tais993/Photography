using System.CommandLine;
using Application.interfaces.services.project;
using Domain.entities;
using static Cli.ExitCodes;

namespace Cli.Commands.MetadataCommand;

public class MetadataCommand : CommandBase
{
    private readonly IProjectMetadataService _projectMetadataService;
    private readonly IProjectResolverService _projectResolverService;

    public MetadataCommand(IProjectMetadataService projectMetadataService, IProjectResolverService projectResolverService)
    {
        _projectMetadataService = projectMetadataService;
        _projectResolverService = projectResolverService;
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
        Project project = _projectResolverService.ResolveProject(Directory.GetCurrentDirectory());

        Console.WriteLine(project);

        return Success;
    }
}