using Application.services.interfaces;
using Infrastructure.database.repositories;

namespace Application.services;

public class FileSearchService : IFileSearchService
{
    private IImageRepository _imagesRepository;

    public FileSearchService(IImageRepository imagesRepository)
    {
        _imagesRepository = imagesRepository;
    }

    public void searchImagesByName(string name)
    {
        throw new NotImplementedException();
    }

    public void searchImagesByNumber(int number)
    {
        throw new NotImplementedException();
    }
}