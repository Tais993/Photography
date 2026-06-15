using Application.services.imageviewers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ImageViewerExtensions
{
    public static IServiceCollection AddImageViewer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string mode = configuration["ImageViewer:Mode"] ?? "Disabled";

        switch (mode.ToLowerInvariant())
        {
            case "irfanview":
            {
                string? path = configuration["ImageViewer:IrfanViewPath"];

                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    services.AddSingleton<IImageViewerService>(
                        new UnavailableImageViewerService(
                            $"IrfanView is configured, but the executable was not found: {path}"));

                    break;
                }

                services.AddScoped<IImageViewerService, IrfanViewService>();
                break;
            }


            case "disabled":
                services.AddSingleton<IImageViewerService>(
                    new UnavailableImageViewerService("Image viewer is disabled in configuration."));
                break;

            default:
                services.AddSingleton<IImageViewerService>(
                    new UnavailableImageViewerService($"Unknown image viewer mode: {mode}"));
                break;
        }

        return services;
    }
}