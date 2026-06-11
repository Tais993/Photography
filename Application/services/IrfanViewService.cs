using System.Diagnostics;
using Application.interfaces;
using Application.services.interfaces;
using Microsoft.Extensions.Configuration;

namespace Application.services;

public class IrfanViewService : IIrfanViewService
{
    private readonly string? _irfanViewPath;
    private readonly IFiles _files;
    private readonly IIrfanViewRepository _irfanViewRepository;

    public IrfanViewService(IIrfanViewRepository irfanViewRepository, IFiles files, IConfiguration config)
    {
        _irfanViewPath = config.GetValue<string>("IrfanViewPath");
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

    public void OpenImage(string fullPath)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = _irfanViewPath,
            Arguments = $"\"{fullPath}\"",
            UseShellExecute = false
        });
    }
}