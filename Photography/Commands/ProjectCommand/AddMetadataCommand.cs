using System.CommandLine;
using Application.interfaces.services;
using Domain.entities;
using static Cli.Commands.CommandOptions;
using static Cli.ExitCodes;

namespace Cli.Commands.ProjectCommand;

public class AddMetadataCommand : CommandBase
{

    private readonly IProjectMetadataService _projectMetadataService;
    private readonly IProjectResolverService _projectResolverService;

    public AddMetadataCommand(IProjectResolverService projectResolverService, IProjectMetadataService projectMetadataService)
    {
        _projectResolverService = projectResolverService;
        _projectMetadataService = projectMetadataService;
    }

    protected override string Name => "metadata-add";
    protected override string Description => "Adds the given metadata to the project";

    private static string MetadataKeyName => "-metadata-key";
    private static string ValueName => "-value";

    protected override void Configure(Command command)
    {
        base.Configure(command);

        command.Arguments.Add(new Argument<string>(MetadataKeyName)
        {
            Description = "The key of the metadata"
        });
        command.Options.Add(new Option<string>(ValueName)
        {
            Description = "The value of the metadata"
        });


        command.Options.Add(ProjectOption);
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

        string? value = parseResult.GetValue<string>(ValueName);

        _projectMetadataService.AddMetadataToProject(projectId, metadata.MetadataKey, value);

        return Success;
    }

    public Metadata? ResolveMetadata(ParseResult parseResult)
    {
        string metadataKey = parseResult.GetValue<string>(MetadataKeyName);

        return _projectMetadataService.GetMetadata(metadataKey);
    }
}