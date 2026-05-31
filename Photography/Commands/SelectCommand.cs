using System.CommandLine;
using Application.services;
using Application.services.interfaces;
using Domain.entities;
using Infrastructure.filesystem;
using Microsoft.Extensions.Logging;
using static Cli.Commands.CommandOptions;

namespace Cli.Commands;

public class SelectCommand : CommandBase
{
    private readonly ISearchService _searchService;
    private readonly IProjectService _projectService;
    private readonly ImageSelectionService _imageSelectionService;
    private readonly IrfanviewService _irfanView;
    private readonly ILogger<SelectCommand> _logger;
    private readonly IFiles _files;


    public SelectCommand(ISearchService searchService, ImageSelectionService imageSelectionService,
        IrfanviewService irfanView, ILogger<SelectCommand> logger, IFiles files, IProjectService projectService)
    {
        _searchService = searchService;
        _imageSelectionService = imageSelectionService;
        _irfanView = irfanView;
        _logger = logger;
        _files = files;
        _projectService = projectService;
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

        string? fileName = _irfanView.GetFileName(parseResult.GetValue<string>(_fileName));
        string? fileNumber = parseResult.GetValue<string>(_fileNumber);

        if (fileName == null && fileNumber == null)
        {
            Console.WriteLine("No file name was given and Irfanview had no file opened.");
        }

        string? fileType = parseResult.GetValue<string>(_fileType);
        string originFolder = parseResult.GetValue<string>(_originFolder)!;

        IEnumerable<Image> images = _searchService.SearchImages(new ImageSearchSettings()
        {
            FileName = fileName,
            FileNumber = fileNumber,
            FileType = fileType,
            FolderName = originFolder,
            ProjectId = project.Id
        });

        SelectionSession selectionSession = _imageSelectionService.GetOrStartSession(project);

        foreach (Image image in images)
        {
            _imageSelectionService.AddImageToSelection((int) selectionSession.Id, (int) image.Id);
        }

        return 0;
    }
}