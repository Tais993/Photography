using System.CommandLine;
using Application.services.interfaces;
using Domain.entities;
using static Cli.Commands.CommandOptions;

namespace Cli.Commands.ProjectCommand;

public class AddMetadataCommand : CommandBase
{

    private readonly IProjectMetadataService _projectMetadataService;
    private readonly IProjectService _projectService;

    public AddMetadataCommand(IProjectService projectService, IProjectMetadataService projectMetadataService)
    {
        _projectService = projectService;
        _projectMetadataService = projectMetadataService;
    }

    protected override string Name => "metadata-add";
    protected override string Description => "Adds the given metadata to the project";

    private static string MetadataIdName => "-metadata-id";
    private static string MetadataKeyName => "-metadata-key";
    private static string ValueName => "-value";

    protected override void Configure(Command command)
    {
        base.Configure(command);

        command.Options.Add(new Option<string>(MetadataKeyName)
        {
            Description = "The key of the metadata"
        });
        command.Options.Add(new Option<int>(MetadataIdName)
        {
            Description = "The ID of the metadata"
        });

        command.Options.Add(new Option<string>(ValueName)
        {
            Description = "The value of the metadata"
        });


        command.Options.Add(ProjectOption);
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

        string? value = parseResult.GetValue<string>(ValueName);

        _projectMetadataService.AddMetadataToProject(projectId, (int)metadata.Id!, value);

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