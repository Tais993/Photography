using Infrastructure.filesystem;
using Infrastructure.irfanview;

namespace Application.services;

public class IrfanviewService
{
    private readonly IFiles _files;
    private readonly IrfanViewRepository _irfanViewRepository;

    public IrfanviewService(IrfanViewRepository irfanViewRepository, IFiles files)
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
}