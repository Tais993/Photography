using System.CommandLine;
using Application.interfaces.services.metadata;
using Application.interfaces.services.project;
using Domain.entities;
using static Cli.ExitCodes;

namespace Cli.Commands.MetadataCommand;

public class MetadataCommand : CommandBase
{
    private readonly IMetadataService _metadataService;
    private readonly IProjectResolverService _projectResolverService;

    public MetadataCommand(IMetadataService metadataService, IProjectResolverService projectResolverService)
    {
        _metadataService = metadataService;
        _projectResolverService = projectResolverService;
    }

    protected override string Name => "metadata";
    protected override string Description => "";
    private static List<string> Aliases => ["md", "meta"];

    protected override void Configure(Command command)
    {
        base.Configure(command);
        AddAliases(Aliases);

        command.Subcommands.Add(new CreateMetadataCommand(_metadataService).Build());
        command.Subcommands.Add(new EditMetadataCommand(_metadataService).Build());
        command.Subcommands.Add(new DeleteMetadataCommand(_metadataService).Build());
    }

    public override int Run(ParseResult parseResult)
    {
        Project project = _projectResolverService.ResolveProject(Directory.GetCurrentDirectory());

        Console.WriteLine(project);

        return Success;
    }
}