using System.CommandLine;
using Application.interfaces.services.project;
using Domain.entities;
using static Cli.Commands.CommandOptions;
using static Cli.ExitCodes;

namespace Cli.Commands.ProjectCommand;

public class RemoveMetadataCommand : CommandBase
{
    private readonly IProjectMetadataService _projectMetadataService;
    private readonly IProjectResolverService _projectResolverService;

    public RemoveMetadataCommand(IProjectResolverService projectResolverService, IProjectMetadataService projectMetadataService)
    {
        _projectResolverService = projectResolverService;
        _projectMetadataService = projectMetadataService;
    }

    private static string MetadataKeyName => "-metadata-key";

    protected override string Name => "metadata-remove";
    protected override string Description => "Removes the given metadata from the project";

    protected override void Configure(Command command)
    {
        base.Configure(command);

        command.Options.Add(ProjectOption);

        command.Options.Add(new Option<string>(MetadataKeyName)
        {
            Description = "The key of the metadata"
        });
    }

    public override int Run(ParseResult parseResult)
    {
        int projectId = _projectResolverService.ResolveProjectId(Directory.GetCurrentDirectory(),
            parseResult.GetValue(ProjectOption));

        Metadata? metadata = ResolveMetadata(parseResult);

        if (projectId == 0)
        {
            return InvalidInput("No valid project was given or resolved.");
        }
        
        if (metadata == null)
        {
            return InvalidInput("No valid metadata-key was given.");
        }

        _projectMetadataService.RemoveMetadataFromProject(projectId, metadata.MetadataKey);
        
        return Success;
    }

    public Metadata? ResolveMetadata(ParseResult parseResult)
    {
        string metadataKey = parseResult.GetValue<string>(MetadataKeyName);

        return _projectMetadataService.GetMetadata(metadataKey);
    }
}