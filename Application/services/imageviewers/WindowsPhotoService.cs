using Application.interfaces;
using Application.interfaces.imageviewers;
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
            return givenFileName;
        }

        if (!_windowsPhotoGateway.IsOpen())
        {
            return null;
        }

        _logger.LogInformation("Checking Windows Photos for an opened image");
        string? openedFile = _windowsPhotoGateway.GetOpenedFile();

        return _files.GetFileNameWithoutExtension(openedFile);

    }

    public void OpenImage(string imagePath)
    {
        if (!IsAvailable())
        {
            throw new InvalidOperationException("Windows Photos is only available on Windows.");
        }
        
        _windowsPhotoGateway.OpenFile(imagePath);
    }
}