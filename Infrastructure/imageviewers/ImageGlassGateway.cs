using System.Diagnostics;
using Application;
using Application.interfaces.infrastructure.imageviewers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.imageviewers;

public class ImageGlassGateway : IImageGlassGateway
{
    private readonly ImageViewerGatewayHelper _imageViewerGatewayHelper;
    private static string[] ProcessNames => ["ImageGlass", "ig"];

    private static string[] TitleSeparators =>
    [
        "\uFE31" // ︱ ImageGlass separator
    ];

    private readonly ILogger<ImageGlassGateway> _logger;
    private readonly string? _imageGlassPath;

    public ImageGlassGateway(ILogger<ImageGlassGateway> logger, IConfiguration configuration,
        ImageViewerGatewayHelper imageViewerGatewayHelper)
    {
        _imageViewerGatewayHelper = imageViewerGatewayHelper;
        _imageGlassPath = configuration.GetValue<string>(Constants.ConfigImageViewerPath);
        _logger = logger;
    }


    public bool IsOpen()
    {
        return _imageViewerGatewayHelper.IsOpen(ProcessNames);
    }

    public string? GetOpenedFile()
    {
        return _imageViewerGatewayHelper.GetOpenedFile(ProcessNames, TitleSeparators);
    }

    public void OpenFile(string imagePath)
    {
        Process.Start(
            _imageViewerGatewayHelper.CreateFileOpenProcess(_imageGlassPath, imagePath)
        );
    }
}