using System.CommandLine;
using Application.interfaces.services;
using static Cli.ExitCodes;

namespace Cli.Commands.MetadataCommand;

public class CreateMetadataCommand : CommandBase
{
    private const string MetadataKey = "-metadata-key";
    private const string MetaDisplayName = "-display-name";
    private const string MetaDescription = "-description";
    private const string MetadataType = "-metadata-type";

    private readonly IProjectMetadataService _projectMetadataService;

    public CreateMetadataCommand(IProjectMetadataService projectMetadataService)
    {
        _projectMetadataService = projectMetadataService;
    }

    protected override string Name => "create";
    protected override string Description => "Create metadata";

    protected override void Configure(Command command)
    {
        command.Arguments.Add(new Argument<string>(MetadataKey));
        command.Arguments.Add(new Argument<string>(MetaDisplayName));
        command.Arguments.Add(new Argument<string>(MetaDescription)
        {
            DefaultValueFactory = _ => string.Empty
        });
        command.Arguments.Add(new Argument<string>(MetadataType)
        {
            DefaultValueFactory = _ => "void"
        });
    }

    public override int Run(ParseResult parseResult)
    {
        string? metadataKey = parseResult.GetValue<string>(MetadataKey);
        string? displayName = parseResult.GetValue<string>(MetaDisplayName);
        string? description = parseResult.GetValue<string>(MetaDescription);
        string? metadataType = parseResult.GetValue<string>(MetadataType);

        if (metadataKey == null || displayName == null)
        {
            return InvalidInput("Metadatakey and or displayname cannot be null");
        }

        _projectMetadataService.CreateMetadata(metadataKey, metadataType, displayName, description);

        return Success;
    }
}