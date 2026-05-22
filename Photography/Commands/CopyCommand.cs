using System.CommandLine;
using Application.services;
using Application.services.interfaces;
using Domain.entities;
using Infrastructure.filesystem;
using Microsoft.Extensions.Logging;
using static Cli.Commands.CommandOptions;

namespace Cli.Commands;

public class CopyCommand : CommandBase
{

    private readonly IFileSearchService _fileSearchService;
    private readonly IProjectService _projectService;
    private readonly ILogger<CopyCommand> _logger;
    private readonly IrfanviewService _irfanView;
    private readonly ICopyService _copyService;
    private readonly IFiles _files;



    public CopyCommand(IFileSearchService fileSearchService, IrfanviewService irfanView, IProjectService projectService,
        IFiles files, ICopyService copyService, ILogger<CopyCommand> logger)
    {
        _fileSearchService = fileSearchService;
        _projectService = projectService;
        _irfanView = irfanView;
        _copyService = copyService;
        _files = files;
        _logger = logger;
    }

    protected override string Name => "copy";
    protected override string Description => "Copy any opened files from irfanview to the editing folder";

    private readonly string _fileName = "-filename";
    private readonly string _fileNumber = "-filenumber";
    private readonly string _fileType = "-filetype";
    private readonly string _destinationFolder = "-destination-folder";
    private readonly string _originFolder = "-origin-folder";

    protected override void Configure(Command command)
    {
        base.Configure(command);

        command.Options.Add(ProjectOption);

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
        command.Options.Add(new Option<string>(_destinationFolder)
        {
            Description = "The destination folder name",
            DefaultValueFactory = _ => "Editing"
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
        string destinationFolder = parseResult.GetValue<string>(_destinationFolder)!;


        IEnumerable<string> images = _fileSearchService.SearchImages(new FileSearchSettings()
        {
            FileName = fileName,
            FileNumber = fileNumber,
            FileType = fileType,
            FolderName = originFolder,
            ProjectId = project.Id
        }).Select(image => image.RelationalFilePath);

        _copyService.CopyFiles(images, project.Path, destinationFolder);

        return 0;
    }
}