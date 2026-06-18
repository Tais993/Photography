using Application.interfaces.infrastructure;
using Application.interfaces.infrastructure.imageviewers;
using Application.interfaces.services;
using Microsoft.Extensions.Logging;

namespace Application.services.imageviewers;

public class WindowsPhotosService : IImageViewerService
{
    private readonly IFiles _files;
    private readonly IWindowsPhotoGateway _windowsPhotoGateway;
    private readonly ILogger<WindowsPhotosService> _logger;

    public WindowsPhotosService(IWindowsPhotoGateway windowsPhotoGateway, IFiles files, ILogger<WindowsPhotosService> logger)
    {
        _windowsPhotoGateway = windowsPhotoGateway;
        _files = files;
        _logger = logger;
    }
    
    public bool IsAvailable()
    {
        return OperatingSystem.IsWindows();
    }

    public string GetImageViewerName()
    {
        return "Windows Photos";
    }

    public string? GetOpenedFileName(string? givenFileName)
    {
        if (givenFileName != null)
        {
            _logger.LogDebug("Using provided file name: {FileName}", givenFileName);
            return givenFileName;
        }

        if (!_windowsPhotoGateway.IsOpen())
        {
            _logger.LogInformation("Windows Photos is not open");
            return null;
        }

        _logger.LogInformation("Checking Windows Photos for an opened image");
        string? openedFile = _windowsPhotoGateway.GetOpenedFile();
        
        _logger.LogInformation("Windows Photos is running, and has: {OpenedFile} opened", openedFile);

        return _files.GetFileNameWithoutExtension(openedFile);

    }

    public void OpenImage(string imagePath)
    {
        if (!IsAvailable())
        {
            _logger.LogWarning("Windows Photos is not available on this operating system");
            throw new InvalidOperationException("Windows Photos is only available on Windows.");
        }
        
        _logger.LogInformation("Opening image in Windows Photos: {ImagePath}", imagePath);
        _windowsPhotoGateway.OpenFile(imagePath);
    }
}