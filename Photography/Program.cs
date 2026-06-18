using Application;
using Cli;
using Infrastructure;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(
    new HostApplicationBuilderSettings
    {
        Args = args,
        ContentRootPath = AppContext.BaseDirectory
    });

builder.AddSharedConfiguration();

builder.Services.AddLogic();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCli();


using IHost app = builder.Build();

app.MigrateDatabase();
app.EnsureMetadataExists();
app.RunCli(args);