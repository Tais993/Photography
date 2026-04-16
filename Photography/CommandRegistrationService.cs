using PhotographyNET.cli;

namespace PhotographyNET;

public class CommandRegistrationService
{
    public static void Register(IServiceCollection services)
    {
        services.AddTransient<SearchCommand>();
        services.AddTransient<InitialiseCommand>();
        services.AddTransient<TestCommand>();
    }
}