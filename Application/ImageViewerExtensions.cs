using Application.interfaces.services;
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
        string? mode = configuration[ConfigImageViewerMode] ?? "default";
        string? path = configuration[ConfigImageViewerPath];


        switch (mode.ToLowerInvariant())
        {
            case "irfanview":
                AddIrfanView(services, path);
                break;
            
            case "image-glass":
                AddImageGlass(services, path);
                break;

            case "default":
                AddWindowsPhotos(services);
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

    private static void AddImageGlass(IServiceCollection services, string? path)
    {
        if (!ExecutableExists(path))
        {
            AddUnavailable(
                services,
                $"Image-glass is configured, but the executable was not found: {path}");

            return;
        }

        services.AddScoped<IImageViewerService, ImageGlassService>();
    }
    
    private static void AddWindowsPhotos(IServiceCollection services)
    {
        if (!OperatingSystem.IsWindows())
        {
            AddUnavailable(
                services,
                "Default viewer is configured, but Windows Photos is not found..");

            return;
        }

        services.AddScoped<IImageViewerService, WindowsPhotosService>();
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