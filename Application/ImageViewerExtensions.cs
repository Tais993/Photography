using Application.services.imageviewers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static Application.Constants;

namespace Application;

public static class ImageViewerExtensions
{
    public static IServiceCollection AddImageViewer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? mode = configuration[ImageViewerMode] ?? "default";
        string? path = configuration[ImageViewerPath];


        switch (mode.ToLowerInvariant())
        {
            case "irfanview":
                AddIrfanView(services, path);
                break;

            case "disabled":
                AddUnavailable(services, "Image viewer is disabled in configuration.");
                break;

            default:
                AddUnavailable(services, $"Unknown image viewer mode: {mode}");
                break;
        }

        return services;
    }

    private static void AddIrfanView(IServiceCollection services, string? path)
    {
        if (!ExecutableExists(path))
        {
            AddUnavailable(
                services,
                $"IrfanView is configured, but the executable was not found: {path}");

            return;
        }

        services.AddScoped<IImageViewerService, IrfanViewService>();
    }

    private static void AddUnavailable(IServiceCollection services, string reason)
    {
        services.AddSingleton<IImageViewerService>(
            new UnavailableImageViewerService(reason));
    }

    private static bool ExecutableExists(string? path)
    {
        return !string.IsNullOrWhiteSpace(path) && File.Exists(path);
    }

}