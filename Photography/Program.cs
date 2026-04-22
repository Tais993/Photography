using Application;
using Cli;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(
                new HostApplicationBuilderSettings
                {
                        Args = args,
                        ContentRootPath = AppContext.BaseDirectory,
                })
        ;

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCli();

using var app = builder.Build();

using var scope = app.Services.CreateScope();
var provider = scope.ServiceProvider;

var rootCommand = CommandFactory.Commands(provider);
rootCommand.Parse(args).Invoke();