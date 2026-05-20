using System.CommandLine;

namespace Cli.Commands;

public abstract class CommandOptions
{
    // GLOBAL USED OPTIONS 
    public const string ProjectName = "-project";

    public static readonly Option<int> ProjectOption = new Option<int>(ProjectName)
    {
        Description = "Looks for the given project",
        Aliases =
        {
            "-p"
        }
    };
}