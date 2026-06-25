using System.CommandLine;
using Application.interfaces.services.metadata;
using static Cli.ExitCodes;

namespace Cli.Commands.MetadataCommand;

public class DeleteMetadataCommand : CommandBase
{
    private const string MetadataKey = "-metadata-key";

    private readonly IProjectMetadataService _projectMetadataService;

    public DeleteMetadataCommand(IProjectMetadataService projectMetadataService)
    {
        _projectMetadataService = projectMetadataService;
    }

    protected override string Name => "delete";
    protected override string Description => "Delete metadata";

    protected override void Configure(Command command)
    {
        command.Arguments.Add(new Argument<string>(MetadataKey));
    }

    public override int Run(ParseResult parseResult)
    {
        string metadataKey = parseResult.GetValue<string>(MetadataKey);

        if (metadataKey is null)
        {
            return InvalidInput("Metadatakey cannot be null");
        }

        Console.WriteLine("Are you sure that you want to delete the metadata with its connection to the projects?");
        Console.WriteLine("This is an irreversible action [Y/N]");

        ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();

        switch (consoleKeyInfo.Key)
        {
            case ConsoleKey.Y:
            {
                Console.WriteLine("Deleting...");
                _projectMetadataService.DeleteMetadata(metadataKey);
                return Success;
            }

            case ConsoleKey.N:
                Console.WriteLine("Cancelled.");
                return Success;
            default:
                Console.WriteLine("Invalid input");
                return ExitCodes.InvalidInput;
        }
    }
}