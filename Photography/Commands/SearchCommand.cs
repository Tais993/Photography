using System.CommandLine;
using Application.services.interfaces;
using Domain.entities;
using Infrastructure.database.repositories;

namespace Cli.Commands;

public class SearchCommand : CommandBase
{
    private const string QueryName = "query";
    private const string ProjectName = "-project";
    private const string GlobalName = "-global";

    private readonly IFileSearchService _fileSearchService;

    private readonly Option<bool> _global = new(GlobalName)
    {
        Description = "Look for an image globally outside of the current projects context",
        Aliases = { "-g" }
    };

    private readonly Option<int> _project = new(ProjectName)
    {
        Description = "Looks for the image within the given project its ID",
        Aliases = { "-p" }
    };

    private readonly IProjectRepository _projectRepository;
    private readonly IProjectService _projectService;

    private readonly Argument<string> _query = new(QueryName)
    {
        Description = "Photo number or filename"
    };

    public SearchCommand(IFileSearchService fileSearchService, IProjectService projectService,
        IProjectRepository projectRepository)
    {
        _fileSearchService = fileSearchService;
        _projectService = projectService;
        _projectRepository = projectRepository;
    }

    protected override string Name => "search";
    protected override string Description => "";

    protected override void Configure(Command command)
    {
        base.Configure(command);

        command.Arguments.Add(_query);
        command.Options.Add(_project);
        command.Options.Add(_global);
    }

    public override int Run(ParseResult parseResult)
    {
        var fileName = parseResult.GetValue<string>(QueryName);
        var shouldByGlobal = parseResult.GetValue<bool>(GlobalName);

        List<Image> images;

        var projectId = ResolveProjectId(parseResult.GetValue(_project));


        if (projectId == 0 || shouldByGlobal)
        {
            images = _fileSearchService.SearchImagesByNameOrNumber(fileName);
        }
        else
        {
            images = _fileSearchService.SearchImagesByNameOrNumber(projectId, fileName);
        }


        foreach (var image in images)
        {
            Console.WriteLine($"File name: `{image.FileName}` Path: `{image.RelationalFilePath}`");
        }

        return 0;
    }

    private int ResolveProjectId(int projectId)
    {
        if (projectId == 0)
        {
            return _projectService.ResolveProjectId(Directory.GetCurrentDirectory());
        }

        return projectId;
    }
}