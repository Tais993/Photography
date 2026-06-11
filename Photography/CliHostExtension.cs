using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cli;

public static class CliHostExtension
{
    extension(IHost app)
    {
        public int RunCli(string[] args)
        {
            using IServiceScope serviceScope = app.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;
        
            RootCommand rootCommand = CommandFactory.Commands(provider);
            return rootCommand.Parse(args).Invoke();
        }
    }   
}