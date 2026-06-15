using System.Diagnostics;
using Application.interfaces.imageviewers;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.imageviewers;

public class WindowsPhotoGateway : IWindowsPhotoGateway
{
    private readonly ImageViewerGatewayHelper _imageViewerGatewayHelper;

    private static string[] ProcessNames =>
    [
        "Photos",
        "Microsoft.Photos",
        "ApplicationFrameHost"
    ];

    private static string[] TitleSeparators => [];

    public WindowsPhotoGateway(ImageViewerGatewayHelper imageViewerGatewayHelper, IConfiguration configuration)
    {
        _imageViewerGatewayHelper = imageViewerGatewayHelper;
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
            new ProcessStartInfo
            {
                FileName = imagePath,
                UseShellExecute = true
            }
        );
    }
}