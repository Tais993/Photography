using Application;
using Cli;
using Infrastructure;
using Infrastructure.database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(
    new HostApplicationBuilderSettings
    {
        Args = args,
        ContentRootPath = AppContext.BaseDirectory
    });

builder.Configuration.AddJsonFile(
    "appsettings.shared.json",
    optional: false,
    reloadOnChange: true
);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCli();

using var app = builder.Build();

using var scope = app.Services.CreateScope();
var provider = scope.ServiceProvider;


// Migration
provider.GetRequiredService<MigrationService>().Migrate();

// Commands
var rootCommand = CommandFactory.Commands(provider);
rootCommand.Parse(args).Invoke();