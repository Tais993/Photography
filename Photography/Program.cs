using Npgsql;
using PhotographyNET;
using PhotographyNET.database;
using PhotographyNET.database.repositories;
using PhotographyNET.services;
using PhotographyNET.services.interfaces;

var builder = WebApplication.CreateBuilder(
        new WebApplicationOptions
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory,
        })
    ;

IServiceCollection services = builder.Services;

// Services
services.AddTransient<ICopyService, CopyService>();
services.AddTransient<IFileSearchService, FileSearchService>();
services.AddTransient<ILightroomService, LightroomService>();
services.AddTransient<IProjectResolver, ProjectResolver>();

// Commands
CommandRegistrationService.Register(builder.Services);


string connectionString = builder.Configuration.GetConnectionString("Default");
// ?? throw new InvalidOperationException("Missing connection string.");
services.AddSingleton(new NpgsqlDataSourceBuilder(connectionString).Build());
services.AddTransient<MigrationService>();
services.AddTransient<RepositoryHelper>();

// Repositories
services.AddTransient<ImageRepository>();
services.AddTransient<ProjectRepository>();
services.AddTransient<MetadataRepository>();
services.AddTransient<ProjectMetadataRepository>();
services.AddTransient<ProjectMetadataAggregateRepository>();


var webApplication = builder.Build();

var provider = webApplication.Services.CreateScope().ServiceProvider;


provider.GetRequiredService<MigrationService>().Migrate();
var rootCommand = CommandFactory.Commands(webApplication.Services);


rootCommand.Parse(args).Invoke();


// // Add services to the container.
// // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();
//
// var app = builder.Build();
//
// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
// }
//
// app.UseHttpsRedirection();
//
// var summaries = new[]
// {
//     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };
//
// app.MapGet("/weatherforecast", () =>
//     {
//         var forecast = Enumerable.Range(1, 5).Select(index =>
//                 new WeatherForecast
//                 (
//                     DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//                     Random.Shared.Next(-20, 55),
//                     summaries[Random.Shared.Next(summaries.Length)]
//                 ))
//             .ToArray();
//         return forecast;
//     })
//     .WithName("GetWeatherForecast");
//
// app.Run();
//
//
// namespace PhotographyNET
// {
//     record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
//     {
//         public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
//     }
// }