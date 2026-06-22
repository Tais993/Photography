using System.CommandLine;
using Application.interfaces.services;
using static Cli.ExitCodes;

namespace Cli.Commands;

public class ApplicationResetCommand : CommandBase
{
    private readonly IApplicationResetService _applicationResetService;
    
    public ApplicationResetCommand(IApplicationResetService applicationResetService)
    {
        this._applicationResetService = applicationResetService;
    }

    protected override string Name => "application-reset";
    protected override string Description => "Clears the whole database, including deletion of all project-info files.";

    private const string  DryRun = "-dry-run";

    protected override void Configure(Command command)
    {
        base.Configure(command);
        
        command.Options.Add(new Option<bool>(DryRun)
        {
            Description = "Whenever to do a dry-run",
            DefaultValueFactory = _ => false
        });
    }

    public override int Run(ParseResult parseResult)
    {
        bool dryRun = parseResult.GetValue<bool>(DryRun);
        
        if (dryRun) Console.WriteLine("Running as a dry-run");

        Console.WriteLine("This deletes all project-info files and clears the whole database.");
        Console.WriteLine("Are you sure? (y/n)");
        ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
        
        switch (consoleKeyInfo.Key)
        {
            case ConsoleKey.Y:
            {
                Console.WriteLine("Deleting...");
                _applicationResetService.ResetApplicationData(dryRun);
                break;
            }
            case ConsoleKey.N: Console.WriteLine("Cancelled."); return Success;
            default: Console.WriteLine("Invalid input"); return ExitCodes.InvalidInput;
        }

        return Success;
    }
}