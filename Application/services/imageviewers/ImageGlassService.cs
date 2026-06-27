using Application.interfaces.infrastructure;
using Application.interfaces.infrastructure.imageviewers;
using Application.interfaces.services;
using Microsoft.Extensions.Logging;
using static Application.Constants;

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
    
    public bool CanOpenFolders()
    {
        return true;
    }

    public void OpenProjectFolder(string folderPath)
    {
        _logger.LogInformation("Opening folder in ImageGlass: {FolderPath}", folderPath);

        if (!_files.Exists(folderPath))
        {
            throw new DirectoryNotFoundException(folderPath);
        }

        string? firstImage = _files
            .GetFiles(folderPath)
            .Where(filePath => ImageFileTypes.Contains(_files.GetFileExtension(filePath)))
            .OrderBy(image => image)
            .FirstOrDefault();

        if (firstImage is null)
        {
            throw new DirectoryNotFoundException("No image files found in folder");
        }
        
        _logger.LogInformation("Opening folder in IrfanView: {FolderPath}", folderPath);
        _imageGlassGateway.OpenFile(firstImage);
    }
}