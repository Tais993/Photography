using System.CommandLine;
using Application.services.interfaces;
using Domain.entities;
using Domain.entities.search;
using static Cli.Commands.CommandOptions;

namespace Cli.Commands;

public class SearchCommand : CommandBase
{
    private readonly ISearchService _searchService;
    private readonly IProjectService _projectService;
 
    private const string QueryName = "query";
    private readonly Argument<string> _queryArgument = new Argument<string>(QueryName)
    {
        Description = "Photo number or filename"
    };
    
    
    private const string GlobalName = "-global";
    private readonly Option<bool> _globalOption = new Option<bool>(GlobalName)
    {
        Description = "Look for an image globally outside of the current projects context",
        Aliases =
        {
            "-g"
        }
    };

    private const string FileTypeName = "-filetype";
    private readonly Option<string> _fileTypeOption = new Option<string>(FileTypeName)
    {
        Description = "The file-type"
    };

    private const string OriginFolderName = "-origin-folder";
    private readonly Option<string> _originFolderOption = new Option<string>(OriginFolderName)
    {
        Description = "The origin folder",
        DefaultValueFactory = _ => "Originals"
    };

    public SearchCommand(ISearchService searchService, IProjectService projectService)
    {
        _searchService = searchService;
        _projectService = projectService;
    }

    protected override string Name => "search";
    protected override string Description => "";

    protected override void Configure(Command command)
    {
        base.Configure(command);

        command.Arguments.Add(_queryArgument);
        command.Options.Add(ProjectOption);
        command.Options.Add(_globalOption);
    }

    public override int Run(ParseResult parseResult)
    {
        string? fileName = parseResult.GetValue<string>(QueryName);
        bool shouldByGlobal = parseResult.GetValue<bool>(GlobalName);
        string? fileType = parseResult.GetValue<string>(FileTypeName);
        string? originFolder = parseResult.GetValue<string>(OriginFolderName);

        int? projectId = null;

        if (!shouldByGlobal)
        {
            projectId = _projectService.ResolveProjectId(Directory.GetCurrentDirectory(),
                parseResult.GetValue(ProjectOption));
        }

        ImageSearchSettings settings = new ImageSearchSettings()
        {
            FileNameOrNumber = fileName,
            ProjectId = projectId,
            FileType = fileType,
            FolderName = originFolder,
        };

        List<Image> images = _searchService.SearchImages(settings);

        foreach (Image image in images)
        {
            Console.WriteLine($"File name: `{image.FileName}` Path: `{image.RelationalFilePath}`");
        }

        return 0;
    }
}