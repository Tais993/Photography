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
        services.AddTransient<IProjectResolver, ProjectService>();

        return services;
    }
}