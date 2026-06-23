using System.CommandLine;
using Application.interfaces.services;
using Application.interfaces.services.project;
using Domain.entities;
using Domain.entities.search;
using Microsoft.Extensions.Logging;
using static Cli.Commands.CommandOptions;
using static Cli.ExitCodes;

namespace Cli.Commands;

public class SelectCommand : CommandBase
{
    private readonly ISearchService _searchService;
    private readonly IProjectResolverService _projectResolverService;
    private readonly IImageSelectionService _imageSelectionService;
    private readonly IImageViewerService _imageViewer;
    private readonly IProjectFolderService _projectFolderService;
    private readonly ILogger<SelectCommand> _logger;


    public SelectCommand(ISearchService searchService, IImageSelectionService imageSelectionService,
        IImageViewerService imageViewer, ILogger<SelectCommand> logger, IProjectFolderService projectFolderService, IProjectResolverService projectResolverService)
    {
        _searchService = searchService;
        _imageSelectionService = imageSelectionService;
        _imageViewer = imageViewer;
        _logger = logger;
        _projectFolderService = projectFolderService;
        _projectResolverService = projectResolverService;
    }

    protected override string Name => "select";
    protected override string Description => "Selects any given image";

    private readonly string _fileName = "-filename";
    private readonly string _fileType = "-filetype";
    private readonly string _fileNumber = "-filenumber";
    private readonly string _originFolder = "-origin-folder";

    protected override void Configure(Command command)
    {
        base.Configure(command);

        command.Options.Add(new Option<string>(_fileName)
        {
            Description = "The file name without the extension"
        });
        command.Options.Add(new Option<string>(_fileNumber)
        {
            Description = "The file number without the extension"
        });
        command.Options.Add(new Option<string>(_fileType)
        {
            Description = "The file extension"
        });
        command.Options.Add(new Option<string>(_originFolder)
        {
            Description = "The origin folder name",
            DefaultValueFactory = _ => "Originals"
        });
    }

    public override int Run(ParseResult parseResult)
    {
        Project? project = _projectResolverService.ResolveProject(Directory.GetCurrentDirectory(),
            parseResult.GetValue(ProjectOption));

        if (project is null)
        {
            return InvalidInput("No valid project was given or resolved.");
        }

        string? fileName = GetFileName(parseResult);
        string? fileNumber = parseResult.GetValue<string>(_fileNumber);

        if (fileName == null && fileNumber == null)
        {
            return InvalidInput("No file name or file number was given.");
        }

        string? fileType = parseResult.GetValue<string>(_fileType);
        string originFolder = parseResult.GetValue<string>(_originFolder)!;
        originFolder = _projectFolderService.ResolveFolder(project, originFolder);

        IEnumerable<Image> images = _searchService.SearchImages(new ImageSearchSettings()
        {
            FileName = fileName,
            FileNumber = fileNumber,
            FileType = fileType,
            FolderName = originFolder,
            ProjectId = project.Id
        }).Items;

        SelectionSession selectionSession = _imageSelectionService.GetOrStartSession(project);

        foreach (Image image in images)
        {
            _logger.LogInformation("Adding image to selection: {ImageFileName}", image.FileName);
            _imageSelectionService.AddImageToSelection((int) selectionSession.Id!, (int) image.Id!);
        }

        return Success;
    }

    private string? GetFileName(ParseResult parseResult)
    {
        string? fileName;

        if (_imageViewer.IsAvailable())
        {
            fileName = _imageViewer.GetOpenedFileName(parseResult.GetValue<string>(_fileName));
        }
        else
        {
            fileName = parseResult.GetValue<string>(_fileName);
        }

        return fileName;
    }
}