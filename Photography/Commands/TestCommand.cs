using System.CommandLine;
using Application.interfaces.services.project;
using Domain.entities;

namespace Cli.Commands;

public class TestCommand : CommandBase
{
    private readonly IProjectResolverService _projectResolverService;
    private readonly IProjectService _projectService;
    private readonly IProjectScanningService _projectScanningService;
    
    public TestCommand(IProjectResolverService projectResolverService, IProjectService projectService, IProjectScanningService projectScanningService)
    {
        _projectResolverService = projectResolverService;
        _projectService = projectService;
        _projectScanningService = projectScanningService;
    }


    protected override string Name => "test";
    protected override string Description => "";


    protected override void Configure(Command command)
    {
        base.Configure(command);
    }

    public override int Run(ParseResult parseResult)
    {
        
        Project? resolvedProject = _projectResolverService.ResolveProject(Directory.GetCurrentDirectory());
        Console.WriteLine(resolvedProject);

        // Project createdProject = _projectService.CreateProject("Temproary stuffz", new DateOnly(2026, 06, 24));
        // Console.WriteLine(createdProject);
        //
        // Project createdSubProject = _projectService.CreateSubProject((int) createdProject.Id!, "Hour 1 of the event");
        // Console.WriteLine(createdSubProject);
        
        _projectScanningService.ScanProject(resolvedProject);
        
        return ExitCodes.Success;
    }
}