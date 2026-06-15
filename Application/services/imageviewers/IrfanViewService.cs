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
            return givenFileName;
        }

        if (!_irfanViewGateway.IsOpen())
        {
            return null;
        }

        _logger.LogInformation("Checking IrfanView for an opened image");
        string? openedFile = _irfanViewGateway.GetOpenedFile();

        return _files.GetFileNameWithoutExtension(openedFile);

    }

    public void OpenImage(string imagePath)
    {
        _irfanViewGateway.OpenFile(imagePath);
    }
}