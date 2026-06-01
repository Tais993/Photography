using System.Diagnostics;
using Infrastructure.filesystem;
using Infrastructure.irfanview;

namespace Application.services;

public class IrfanviewService : IIrfanviewService
{
    private readonly IFiles _files;
    private readonly IIrfanViewRepository _irfanViewRepository;

    public IrfanviewService(IIrfanViewRepository irfanViewRepository, IFiles files)
    {
        _irfanViewRepository = irfanViewRepository;
        _files = files;
    }



    public string? GetFileName(string? givenFileName)
    {
        if (givenFileName != null)
        {
            return givenFileName;
        }

        if (_irfanViewRepository.IsOpen())
        {
            string? openedFile = _irfanViewRepository.GetOpenedFile();

            return _files.GetFileNameWithoutExtension(openedFile);
        }

        return null;
    }

    public void OpenImage(string irfanviewPath, string fullPath)
    {
        string irfanViewPath = @"C:\Program Files\IrfanView\i_view64.exe";
        string imagePath = @"C:\Users\tijs\OneDrive\Desktop\Backup important\Picturestesting\2026-01-29-Selena\Original\DSC_7654.JPG";

        Process.Start(new ProcessStartInfo
        {
            FileName = irfanviewPath,
            Arguments = $"\"{fullPath}\"",
            UseShellExecute = false
        });
    }
}