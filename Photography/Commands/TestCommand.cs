using System.CommandLine;
using Application.services.interfaces;

namespace Cli.Commands;

public class TestCommand : CommandBase
{
    private readonly IProjectResolver projectResolver;

    public TestCommand(IProjectResolver projectResolver)
    {
        this.projectResolver = projectResolver;
    }


    protected override string Name => "test";
    protected override string Description => "";


    protected override void Configure(Command command)
    {
        base.Configure(command);
    }

    public override int Run(ParseResult parseResult)
    {
        var project = projectResolver.resolveProject(Directory.GetCurrentDirectory());


        Console.WriteLine(project);

        return 0;
    }
}