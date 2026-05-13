using System.CommandLine;
using Application.services.interfaces;

namespace Cli.Commands;

public class TestCommand : CommandBase
{
    private readonly IProjectService _projectService;

    public TestCommand(IProjectService projectService)
    {
        this._projectService = projectService;
    }


    protected override string Name => "test";
    protected override string Description => "";


    protected override void Configure(Command command)
    {
        base.Configure(command);
    }

    public override int Run(ParseResult parseResult)
    {
        var project = _projectService.ResolveProject(Directory.GetCurrentDirectory());


        Console.WriteLine(project);

        return 0;
    }
}