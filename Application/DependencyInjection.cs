using Application.services;
using Application.services.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddTransient<ICopyService, CopyService>();
        services.AddTransient<IFileSearchService, FileSearchService>();
        services.AddTransient<ILightroomService, LightroomService>();
        services.AddTransient<IProjectService, ProjectService>();
        services.AddTransient<IProjectMetadataService, ProjectMetadataService>();

        services.AddTransient<IrfanviewService>();
        services.AddTransient<ImageSelectionService>();

        return services;
    }
}