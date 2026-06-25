using System.CommandLine;
using Application.interfaces.services.metadata;
using Domain.entities;
using static Cli.ExitCodes;

namespace Cli.Commands.MetadataCommand;

public class EditMetadataCommand : CommandBase
{
    private const string MetadataKey = "-metadata-key";
    private const string MetaDisplayName = "-display-name";
    private const string MetaDescription = "-description";
    private const string MetadataType = "-metadata-type";

    private readonly IProjectMetadataService _projectMetadataService;

    public EditMetadataCommand(IProjectMetadataService projectMetadataService)
    {
        _projectMetadataService = projectMetadataService;
    }

    protected override string Name => "edit";
    protected override string Description => "Edit metadata";

    protected override void Configure(Command command)
    {
        command.Arguments.Add(new Argument<string>(MetadataKey));

        command.Options.Add(new Option<string>(MetaDisplayName));
        command.Options.Add(new Option<string>(MetaDescription));
        command.Options.Add(new Option<string>(MetadataType));
    }

    public override int Run(ParseResult parseResult)
    {
        string? metadataKey = parseResult.GetValue<string>(MetadataKey);

        string? displayName = parseResult.GetValue<string>(MetaDisplayName);
        string? description = parseResult.GetValue<string>(MetaDescription);
        string? metadataType = parseResult.GetValue<string>(MetadataType);

        Metadata? metadata = _projectMetadataService.GetMetadata(metadataKey);

        if (metadataKey is null || metadata is null)
        {
            return InvalidInput("Metadatakey is null or invalid");
        }

        metadata.DisplayName = displayName ?? metadata.DisplayName;
        metadata.Description = description ?? metadata.Description;
        metadata.MetadataType = metadataType ?? metadata.MetadataType;

        _projectMetadataService.UpdateMetadata(metadata);

        Console.WriteLine("Updated metadata in the database");

        return Success;
    }
}