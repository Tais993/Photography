using Application.interfaces;

namespace Application.services.imageviewers;

public class IrfanViewService : IImageViewerService
{
    private readonly IFiles _files;
    private readonly IIrfanViewGateway _irfanViewGateway;

    public IrfanViewService(IIrfanViewGateway irfanViewGateway, IFiles files)
    {
        _irfanViewGateway = irfanViewGateway;
        _files = files;
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

        if (_irfanViewGateway.IsOpen())
        {
            string? openedFile = _irfanViewGateway.GetOpenedFile();

            return _files.GetFileNameWithoutExtension(openedFile);
        }

        return null;
    }

    public void OpenImage(string imagePath)
    {
        _irfanViewGateway.OpenFile(imagePath);
    }
}