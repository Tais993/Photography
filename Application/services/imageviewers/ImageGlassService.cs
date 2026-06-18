using Application.interfaces;
using Application.interfaces.imageviewers;
using Microsoft.Extensions.Logging;

namespace Application.services.imageviewers;

public class ImageGlassService : IImageViewerService
{
    private readonly IFiles _files;
    private readonly IImageGlassGateway _imageGlassGateway;
    private readonly ILogger<ImageGlassService> _logger;

    public ImageGlassService(IFiles files, ILogger<ImageGlassService> logger, IImageGlassGateway imageGlassGateway)
    {
        _imageGlassGateway = imageGlassGateway;
        _logger = logger;
        _files = files;
    }

    public bool IsAvailable()
    {
        return true;
    }

    public string GetImageViewerName()
    {
        return "ImageGlass";
    }

    public string? GetOpenedFileName(string? givenFileName)
    {
        if (givenFileName != null)
        {
            _logger.LogDebug("Using provided file name: {FileName}", givenFileName);
            return givenFileName;
        }

        if (!_imageGlassGateway.IsOpen())
        {
            _logger.LogDebug("ImageGlass is not open");
            return null;
        }

        _logger.LogDebug("Checking ImageGlass for an opened image");
        string? openedFile = _imageGlassGateway.GetOpenedFile();

        _logger.LogInformation("ImageGlass is running, and has: {OpenedFile} opened", openedFile);
        
        return _files.GetFileNameWithoutExtension(openedFile);

    }
    public void OpenImage(string imagePath)
    {
        _logger.LogInformation("Opening image in ImageGlass: {ImagePath}", imagePath);
        _imageGlassGateway.OpenFile(imagePath);
    }
}