using System.Diagnostics;
using Application;
using Application.interfaces.imageviewers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.imageviewers;

public class IrfanViewGateway : IIrfanviewGateway
{
    private readonly ImageViewerGatewayHelper _imageViewerGatewayHelper;
    private static string[] ProcessNames => ["i_view64", "i_view32"];
    private static string[] TitleSeparators => [" - IrfanView"];
    private readonly ILogger<IrfanViewGateway> _logger;
    private readonly string? _irfanViewPath;

    public IrfanViewGateway(ILogger<IrfanViewGateway> logger, IConfiguration configuration,
        ImageViewerGatewayHelper imageViewerGatewayHelper)
    {
        _imageViewerGatewayHelper = imageViewerGatewayHelper;
        _irfanViewPath = configuration.GetValue<string>(Constants.ImageViewerPath);
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
            _imageViewerGatewayHelper.CreateFileOpenProcess(_irfanViewPath, imagePath)
        );
    }
}