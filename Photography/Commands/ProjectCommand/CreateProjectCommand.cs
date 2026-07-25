using System.CommandLine;
using Application.interfaces.services;
using Application.interfaces.services.project;
using Application.interfaces.website;
using Cli.utils;
using Domain.entities;
using Microsoft.Extensions.Logging;
using static Cli.ExitCodes;
using static Cli.utils.CameraDriveSelector;

namespace Cli.Commands.ProjectCommand;

public class CreateProjectCommand : CommandBase
{
    private readonly ILogger<CreateProjectCommand> _logger;
    private readonly IProjectService _projectService;
    private readonly IProjectImportService _projectImportService;
    private readonly IProjectIndexService _projectIndexService;
    private readonly ICameraDriveService _cameraDriveService;

    public CreateProjectCommand(IProjectService projectService, IProjectImportService projectImportService,
        IProjectIndexService projectIndexService, ICameraDriveService cameraDriveService, ILogger<CreateProjectCommand> logger)
    {
        _projectService = projectService;
        _projectImportService = projectImportService;
        _projectIndexService = projectIndexService;
        _cameraDriveService = cameraDriveService;
        _logger = logger;
    }

    protected override string Name => "create";
    protected override string Description => "Create a new project";

    private Guid importId;
    private ProgressBar? _progressBar;

    protected override void Configure(Command command)
    {
        base.Configure(command);
    }

    public override int Run(ParseResult parseResult)
    {
        Dictionary<int, LogicalDrive> importDriveOptions = _cameraDriveService.GetImportDrives()
            .Select((drive, index) => new KeyValuePair<int, LogicalDrive>(index + 1, drive))
            .Where(keyValue => keyValue.Value.HasDcimFolder)
            .ToDictionary();

        if (importDriveOptions.Count == 0)
        {
            Console.WriteLine("No import drives found.");
            return Failure;
        }

        DisplayImportDrives(importDriveOptions);

        if (!TryGetPickedDrive(importDriveOptions, out LogicalDrive pickedDrive))
        {
            return Failure;
        }

        Console.WriteLine($"Selected drive: {pickedDrive.GetDisplayName()}");

        DateOnly firstPhotoDate = DateOnly.FromDateTime(_cameraDriveService.GetFirstPhotoDate(pickedDrive) ?? DateTime.Today);

        if (!VerifyPhotoDate(firstPhotoDate))
        {
            // ask date
        }


        string projectName = GetProjectName();

        if (!ConfirmProjectCreation(projectName, firstPhotoDate, pickedDrive))
        {
            Console.WriteLine("Project creation cancelled.");
            return Success;
        }

        List<string> files = _cameraDriveService.GetFilesToCopy(pickedDrive!);

        _logger.LogDebug("Found {Count} files to import", files.Count);

        Project project = _projectService.CreateProject(projectName, firstPhotoDate);

        _logger.LogDebug("Should start import for project {ProjectId}", project.Id);
        importId = _projectImportService.StartImport(new ProjectImportRequest()
        {
            FilePaths = files,
            ProjectId = (int)project.Id!,
            RemoveSourceFilesAfterImport = true
        });

        _progressBar = new ProgressBar();
        while (true)
        {
            int result = UpdateProgress();

            if (result == -1)
            {
                continue;
            }

            return result;
        }
    }

    private int UpdateProgress()
    {
        ProjectImportProgress? progress =
            _projectImportService.GetProgress(importId);

        if (progress is null)
        {
            Thread.Sleep(100);
            return -1;
        }

        // ProjectImportProgress uses 0–100;
        // ProgressBar expects 0.0–1.0.
        _progressBar!.Report(progress.Percentage / 100.0);

        if (progress.HasFailed)
        {
            Console.WriteLine();
            Console.WriteLine("Import failed.");
            return Failure;
        }

        if (progress.IsCompleted)
        {
            Console.WriteLine();
            Console.WriteLine("Import completed successfully.");
            return Success;
        }

        Thread.Sleep(100);
        return -1;
    }

    private static bool ConfirmProjectCreation(string projectName, DateOnly? firstPhotoDate, LogicalDrive pickedDrive)
    {
        Console.WriteLine(
            $"Project name: {projectName}, project date: {firstPhotoDate}, selected drive: {pickedDrive.GetDisplayName()}");
        Console.WriteLine("Are you sure? (y/n):");
        ConsoleKey consoleKeyInfo = Console.ReadKey().Key;

        switch (consoleKeyInfo)
        {
            case ConsoleKey.N:
                return false;
            case ConsoleKey.Y:
                return true;
            default:
                Console.WriteLine("Invalid input");
                return false;
        }
    }

    private static bool VerifyPhotoDate(DateOnly? firstPhotoDate)
    {
        Console.WriteLine($"First photo date: {firstPhotoDate}");
        Console.Write("Is this correct? (y/n): ");

        ConsoleKey inputKey = Console.ReadKey().Key;

        switch (inputKey)
        {
            case ConsoleKey.N:
                return false;
            case ConsoleKey.Y:
                return true;
            default:
                Console.WriteLine("Invalid input");
                return false;
        }
    }


    private string GetProjectName()
    {
        Console.WriteLine("");
        Console.Write("Project Name: ");
        return Console.ReadLine() ?? "";
    }
}