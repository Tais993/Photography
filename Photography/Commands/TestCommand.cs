using System.CommandLine;
using Application.interfaces.services;
using Domain.entities;

namespace Cli.Commands;

public class TestCommand : CommandBase
{
    private readonly IProjectResolverService _projectResolverService;

    public TestCommand(IProjectResolverService projectResolverService)
    {
        _projectResolverService = projectResolverService;
    }


    protected override string Name => "test";
    protected override string Description => "";


    protected override void Configure(Command command)
    {
        base.Configure(command);
    }

    public override int Run(ParseResult parseResult)
    {
        Project? project = _projectResolverService.ResolveProject(Directory.GetCurrentDirectory());
        
        Console.WriteLine(project);

        return ExitCodes.Success;
    }
}