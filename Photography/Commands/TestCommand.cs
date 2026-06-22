using System.CommandLine;
using Application.interfaces.services;
using Domain.entities;

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
        Project? project = _projectService.ResolveProject(Directory.GetCurrentDirectory());
        
        Console.WriteLine(project);

        return ExitCodes.Success;
    }
}