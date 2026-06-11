using System.CommandLine;
using Application.services.interfaces;
using Domain.entities;
using Microsoft.Extensions.Logging;
using static Cli.Commands.CommandOptions;

namespace Cli.Commands;

public class CopyCommand : CommandBase
{

    private readonly IImageSelectionService _imageSelectionService;
    private readonly ILogger<CopyCommand> _logger;
    private readonly IProjectService _projectService;
    private readonly ICopyService _copyService;



    public CopyCommand(IProjectService projectService, ICopyService copyService, ILogger<CopyCommand> logger, IImageSelectionService imageSelectionService)
    {
        _projectService = projectService;
        _copyService = copyService;
        _logger = logger;
        _imageSelectionService = imageSelectionService;
    }

    protected override string Name => "copy";
    protected override string Description => "Copy any opened files from irfanview to the editing folder";

    private readonly string _destinationFolder = "-destination-folder";

    protected override void Configure(Command command)
    {
        base.Configure(command);

        command.Options.Add(ProjectOption);
        
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

        string destinationFolder = parseResult.GetValue<string>(_destinationFolder)!;

        
        SelectionSession selectionSession = _imageSelectionService.GetSessionImages(project);

        List<int> imageIds = selectionSession.ImageIds;

        IEnumerable<string> imagePaths = _copyService.ImageIdsToRelativePaths(imageIds.ToArray());
        
        _copyService.CopyFiles(imagePaths, project.Path, destinationFolder);

        _imageSelectionService.ClearSession(project);
        
        return 0;
    }
}