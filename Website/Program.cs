using Application;
using Infrastructure;
using Website;

WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions {
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,
});

builder.AddSharedConfiguration();

builder.Services.AddWebsite();
builder.Services.AddLogic();
builder.Services.AddInfrastructure(builder.Configuration);


WebApplication app = builder.Build();


app.MigrateDatabase();
app.EnsureMetadataExists();
app.RunWeb();