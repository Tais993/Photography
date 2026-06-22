using System.CommandLine;
using Application.interfaces.services;
using Domain.entities;
using Microsoft.Extensions.Logging;
using static Cli.Commands.CommandOptions;
using static Cli.ExitCodes;

namespace Cli.Commands;

public class CopyCommand : CommandBase
{

    private readonly IImageSelectionService _imageSelectionService;
    private readonly ILogger<CopyCommand> _logger;
    private readonly IProjectResolverService _projectResolverService;
    private readonly ICopyService _copyService;
    private readonly IProjectFolderService _projectFolderService;


    public CopyCommand(ICopyService copyService, ILogger<CopyCommand> logger, IImageSelectionService imageSelectionService, IProjectFolderService projectFolderService, IProjectResolverService projectResolverService)
    {
        _copyService = copyService;
        _logger = logger;
        _imageSelectionService = imageSelectionService;
        _projectFolderService = projectFolderService;
        _projectResolverService = projectResolverService;
    }

    protected override string Name => "copy";
    protected override string Description => "Copy any opened files from your image viewer to the editing folder";

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
        Project? project = _projectResolverService.ResolveProject(Directory.GetCurrentDirectory(),
            parseResult.GetValue(ProjectOption));

        if (project is null)
        {
            return InvalidInput("No valid project was given or resolved.");
        }
        
        string destinationFolder = parseResult.GetValue<string>(_destinationFolder)!;
        destinationFolder = _projectFolderService.ResolveFolder(project!, destinationFolder);

        
        SelectionSession selectionSession = _imageSelectionService.GetOrStartSession(project);

        
        List<int> imageIds = selectionSession.ImageIds;
        IEnumerable<string> imagePaths = _copyService.ImageIdsToRelativePaths(imageIds.ToArray());
        
        _copyService.CopyFiles(imagePaths, project.Path, destinationFolder);
        _imageSelectionService.ClearSession(project);
        
        return Success;
    }
}