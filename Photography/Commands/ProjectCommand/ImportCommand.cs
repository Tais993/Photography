using System.CommandLine;
using Application.interfaces.services;
using Application.interfaces.services.project;
using Cli.utils;
using Domain.entities;
using Microsoft.Extensions.Logging;
using static Cli.Commands.CommandOptions;
using static Cli.ExitCodes;
using static Cli.utils.CameraDriveSelector;

namespace Cli.Commands.ProjectCommand;

public class ImportCommand : CommandBase
{
    private readonly IProjectImportService _projectImportService;
    private readonly ICameraDriveService _cameraDriveService;
    private readonly IProjectResolverService _projectResolverService;
    private readonly IProjectService _projectService;
    private readonly ILogger<ImportCommand> _logger;

    public ImportCommand(IProjectImportService projectImportService, IProjectResolverService projectResolverService, IProjectService projectService, ICameraDriveService cameraDriveService, ILogger<ImportCommand> logger)
    {
        this._projectImportService = projectImportService;
        this._projectResolverService = projectResolverService;
        this._projectService = projectService;
        _cameraDriveService = cameraDriveService;
        _logger = logger;
    }

    protected override string Name => "import";
    protected override string Description => "Import recognizes drives into any given project";
    
    private Guid importId;
    private ProgressBar? _progressBar;

    protected override void Configure(Command command)
    {
        base.Configure(command);
        
        command.Options.Add(ProjectOption);
    }

    public override int Run(ParseResult parseResult)
    {
        int projectId = _projectResolverService.ResolveProjectId(Directory.GetCurrentDirectory(),
            parseResult.GetValue(ProjectOption));

        Dictionary<int, LogicalDrive> importDriveOptions = _cameraDriveService.GetImportDrives()
            .Where(drive => drive.HasDcimFolder)
            .Select((drive, index) => new KeyValuePair<int, LogicalDrive>(index + 1, drive))
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
        
        
        List<string> files = _cameraDriveService.GetFilesToCopy(pickedDrive!);

        _logger.LogDebug("Found {Count} files to import", files.Count);
        _logger.LogDebug("Should start import for project {ProjectId}", projectId);
        
        importId = _projectImportService.StartImport(new ProjectImportRequest()
        {
            FilePaths = files,
            ProjectId = projectId,
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
}