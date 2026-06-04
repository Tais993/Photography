using System.CommandLine;
using Application.services.interfaces;
using Domain.entities;

namespace Cli.Commands;

public class MetadataCommand : CommandBase
{

    private const string MetadataKey = "-metadata-key";
    private const string MetaDisplayName = "-display-name";
    private const string MetaDescription = "-description";
    private const string MetadataType = "-metadata-type";
    private readonly IProjectMetadataService _projectMetadataService;
    private readonly IProjectService _projectService;

    public MetadataCommand(IProjectService projectService, IProjectMetadataService projectMetadataService)
    {
        _projectService = projectService;
        _projectMetadataService = projectMetadataService;
    }


    protected override string Name => "metadata";
    protected override string Description => "";
    private static List<string> aliases => ["md", "meta"];


    protected override void Configure(Command command)
    {
        base.Configure(command);
        AddAliases(aliases);

        Command createCommand = new Command("create", "Create metadata")
        {
            Arguments =
            {
                new Argument<string>(MetadataKey),
                new Argument<string>(MetaDisplayName),
                new Argument<string>(MetaDescription)
                {
                    DefaultValueFactory = _ => string.Empty
                },
                new Argument<string>(MetadataType)
                {
                    DefaultValueFactory = _ => "void"
                }
            }
        };
        createCommand.SetAction(CreateMetadata);

        Command editCommand = new Command("edit", "Edit metadata")
        {
            Arguments =
            {
                new Argument<string>(MetadataKey)
            },
            Options =
            {
                new Option<string>(MetaDisplayName),
                new Option<string>(MetaDescription),
                new Option<string>(MetadataType)
            }
        };
        editCommand.SetAction(EditMetadata);

        Command deleteCommand = new Command("delete", "Delete metadata")
        {
            Arguments =
            {
                new Argument<string>(MetadataKey)
                
            }
        };
        deleteCommand.SetAction(DeleteMetadata);


        command.Subcommands.Add(createCommand);
        command.Subcommands.Add(editCommand);
        command.Subcommands.Add(deleteCommand);
    }

    public override int Run(ParseResult parseResult)
    {
        Project project = _projectService.ResolveProject(Directory.GetCurrentDirectory());


        Console.WriteLine(project);

        return 0;
    }

    private int CreateMetadata(ParseResult parseResult)
    {
        string? metadataKey = parseResult.GetValue<string>(MetadataKey);
        string? displayName = parseResult.GetValue<string>(MetaDisplayName);
        string? description = parseResult.GetValue<string>(MetaDescription);
        string? metadataType = parseResult.GetValue<string>(MetadataType);

        if (metadataKey == null || displayName == null)
        {
            Console.WriteLine("Metadatakey and or displayname cannot be null");
            return -1;
        }

        _projectMetadataService.CreateMetadata(metadataKey, metadataType, displayName, description);

        return 0;
    }

    private int EditMetadata(ParseResult parseResult)
    {
        string metadataKey = parseResult.GetValue<string>(MetadataKey);


        string? displayName = parseResult.GetValue<string>(MetaDisplayName);
        string? description = parseResult.GetValue<string>(MetaDescription);
        string? metadataType = parseResult.GetValue<string>(MetadataType);

        Metadata? metadata = _projectMetadataService.GetMetadata(metadataKey);

        if (metadata == null)
        {
            Console.WriteLine("Given metadata ID is invalid or cannot be found");
            return -1;
        }

        metadata.MetadataKey = metadataKey ?? metadata.MetadataKey;
        metadata.DisplayName = displayName ?? metadata.DisplayName;
        metadata.Description = description ?? metadata.Description;
        metadata.MetadataType = metadataType ?? metadata.MetadataType;

        _projectMetadataService.UpdateMetadata(metadata);

        Console.WriteLine("Updated metadata in the database");

        return 0;
    }

    private int DeleteMetadata(ParseResult parseResult)
    {
        string metadataKey = parseResult.GetValue<string>(MetadataKey);

        Console.WriteLine("Are you sure that you want to delete the metadata with its connection to the projects?");
        Console.WriteLine("This is NOT a reversable action [Y/N]");

        string? input = Console.ReadLine();

        if (!string.Equals(input, "y", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(input, "yes", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Cancelled.");
            return 0;
        }

        _projectMetadataService.DeleteMetadata(metadataKey);

        return 0;
    }
}