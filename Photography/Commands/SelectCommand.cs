using System.CommandLine;
using Application.interfaces.services;
using Domain.entities;
using Domain.entities.search;
using Microsoft.Extensions.Logging;
using static Cli.Commands.CommandOptions;

namespace Cli.Commands;

public class SelectCommand : CommandBase
{
    private readonly ISearchService _searchService;
    private readonly IProjectService _projectService;
    private readonly IImageSelectionService _imageSelectionService;
    private readonly IImageViewerService _imageViewer;
    private readonly IProjectFolderService _projectFolderService;
    private readonly ILogger<SelectCommand> _logger;


    public SelectCommand(ISearchService searchService, IImageSelectionService imageSelectionService,
        IImageViewerService imageViewer, ILogger<SelectCommand> logger, IProjectService projectService, IProjectFolderService projectFolderService)
    {
        _searchService = searchService;
        _imageSelectionService = imageSelectionService;
        _imageViewer = imageViewer;
        _logger = logger;
        _projectService = projectService;
        _projectFolderService = projectFolderService;
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
        Project? project = _projectService.ResolveProject(Directory.GetCurrentDirectory(),
            parseResult.GetValue(ProjectOption));

        if (project is null)
        {
            Console.WriteLine("No project found");
            return 1;
        }

        string? fileName = GetFileName(parseResult);
        string? fileNumber = parseResult.GetValue<string>(_fileNumber);

        if (fileName == null && fileNumber == null)
        {
            Console.WriteLine("No file name was given.");
            return 1;
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
            _imageSelectionService.AddImageToSelection((int)selectionSession.Id, (int)image.Id);
        }

        return 0;
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