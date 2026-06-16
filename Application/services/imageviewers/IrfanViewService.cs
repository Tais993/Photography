using Application.interfaces;
using Application.interfaces.imageviewers;
using Microsoft.Extensions.Logging;

namespace Application.services.imageviewers;

public class IrfanViewService : IImageViewerService
{
    private readonly IFiles _files;
    private readonly IIrfanviewGateway _irfanViewGateway;
    private readonly ILogger<IrfanViewService> _logger;

    public IrfanViewService(IIrfanviewGateway irfanViewGateway, IFiles files, ILogger<IrfanViewService> logger)
    {
        _irfanViewGateway = irfanViewGateway;
        _files = files;
        _logger = logger;
    }


    public bool IsAvailable()
    {
        return true;
    }

    public string GetImageViewerName()
    {
        return "IrfanView";
    }

    public string? GetOpenedFileName(string? givenFileName)
    {
        if (givenFileName != null)
        {
            _logger.LogDebug("Using provided file name: {FileName}", givenFileName);
            return givenFileName;
        }

        if (!_irfanViewGateway.IsOpen())
        {
            _logger.LogDebug("IrfanView is not open");
            return null;
        }

        _logger.LogDebug("Checking IrfanView for an opened image");
        string? openedFile = _irfanViewGateway.GetOpenedFile();

        _logger.LogDebug("IrfanView opened file: {OpenedFile}", openedFile);

        return _files.GetFileNameWithoutExtension(openedFile);

    }

    public void OpenImage(string imagePath)
    {
        _logger.LogInformation("Opening image in IrfanView: {ImagePath}", imagePath);
        _irfanViewGateway.OpenFile(imagePath);
    }
}