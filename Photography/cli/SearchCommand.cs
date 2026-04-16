using System.CommandLine;

namespace PhotographyNET.cli;

public class SearchCommand : CommandBase
{

    protected override string Name => "search";
    protected override string Description => "";
    
    private readonly Argument<string> _query = new("query")
    {
        Description =  "Photo number or filename fragment"
    };
    private readonly Option<bool> _exact = new("--exact")
    {
        Description = "Use exact matching only"
    };

    protected override void Configure(Command command)
    {
        base.Configure(command);
        
        command.Arguments.Add(_query);
        command.Options.Add(_exact);
    }

    public override int Run(ParseResult parseResult)
    {
        throw new NotImplementedException();
    }
}