using System.CommandLine;
using Application.services.interfaces;
using Domain.entities;
using Infrastructure.database.repositories;
using static Cli.Commands.CommandOptions;

namespace Cli.Commands;

public class SearchCommand : CommandBase
{
    public const string QueryGlobalName = "-global";
    public const string QueryName = "query";
    private readonly IFileSearchService _fileSearchService;

    private readonly Option<bool> _global = new Option<bool>(QueryGlobalName)
    {
        Description = "Look for an image globally outside of the current projects context",
        Aliases =
        {
            "-g"
        }
    };

    private readonly IProjectRepository _projectRepository;
    private readonly IProjectService _projectService;

    private readonly Argument<string> _query = new Argument<string>(QueryName)
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
        command.Options.Add(ProjectOption);
        command.Options.Add(_global);
    }

    public override int Run(ParseResult parseResult)
    {
        string? fileName = parseResult.GetValue<string>(QueryName);
        bool shouldByGlobal = parseResult.GetValue<bool>(QueryGlobalName);

        List<Image> images;

        int projectId = _projectService.ResolveProjectId(Directory.GetCurrentDirectory(),
            parseResult.GetValue(ProjectOption));


        if (projectId == 0 || shouldByGlobal)
        {
            images = _fileSearchService.SearchImagesByNameOrNumber(fileName);
        }
        else
        {
            images = _fileSearchService.SearchImagesByNameOrNumber(projectId, fileName);
        }


        foreach (Image image in images)
        {
            Console.WriteLine($"File name: `{image.FileName}` Path: `{image.RelationalFilePath}`");
        }

        return 0;
    }
}