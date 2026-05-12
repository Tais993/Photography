using System.CommandLine;
using Application.services.interfaces;
using Domain.entities;

namespace Cli.Commands;

public class SearchCommand : CommandBase
{
    private readonly IFileSearchService _fileSearchService;
    private readonly IProjectResolver _projectResolver;
    
    public SearchCommand(IFileSearchService fileSearchService, IProjectResolver projectResolver)
    {
        this._fileSearchService = fileSearchService;
        this._projectResolver = projectResolver;
    }

    protected override string Name => "search";
    protected override string Description => "";
    
    private static readonly string _queryName = "query";
    private readonly Argument<string> _query = new(_queryName)
    {
        Description =  "Photo number or filename fragment"
    };
    
    private static readonly string _exactName = "exact";
    private readonly Option<bool> _exact = new("exact")
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
        string fileName = parseResult.GetValue<String>(_queryName);


        List<Image> images = _fileSearchService.SearchImagesByNameOrNumber(fileName);

        foreach (var image in images)
        {
            Console.WriteLine($"File name: `{image.FileName}` Path: `{image.RelationalFilePath}`");
        }

        return 0;
    }
}