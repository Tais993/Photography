using System.CommandLine;
using Application.services.interfaces;
using Domain.entities;
using static Cli.Commands.CommandOptions;

namespace Cli.Commands.ProjectCommand;

public class RemoveMetadataCommand : CommandBase
{
    private readonly IProjectMetadataService _projectMetadataService;
    private readonly IProjectService _projectService;

    public RemoveMetadataCommand(IProjectService projectService, IProjectMetadataService projectMetadataService)
    {
        _projectService = projectService;
        _projectMetadataService = projectMetadataService;
    }

    private static string MetadataIdName => "-metadata-id";
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
        command.Options.Add(new Option<int>(MetadataIdName)
        {
            Description = "The ID of the metadata"
        });
    }

    public override int Run(ParseResult parseResult)
    {
        int projectId = _projectService.ResolveProjectId(Directory.GetCurrentDirectory(),
            parseResult.GetValue(ProjectOption));

        Metadata? metadata = ResolveMetadata(parseResult);

        if (metadata == null)
        {
            Console.WriteLine("No valid metadata-key or metadata-id was given");
            return 2;
        }

        _projectMetadataService.RemoveMetadataFromProject(projectId, (int)metadata.Id);

        return 0;
    }

    public Metadata? ResolveMetadata(ParseResult parseResult)
    {
        string metadataKey = parseResult.GetValue<string>(MetadataKeyName);
        int metadataId = parseResult.GetValue<int>(MetadataIdName);

        if (metadataKey != null)
        {
            return _projectMetadataService.GetMetadata(metadataKey);
        }

        return _projectMetadataService.GetMetadata(metadataId);
    }
}