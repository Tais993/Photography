using Application.services.interfaces;
using Domain.entities;
using Infrastructure.database.repositories;

namespace Application.services;

public class FileSearchService : IFileSearchService
{
    private IImageRepository _imagesRepository;

    public FileSearchService(IImageRepository imagesRepository)
    {
        _imagesRepository = imagesRepository;
    }

    public List<Image> searchImagesByName(string name)
    {
        return _imagesRepository.GetImagesByFileName(name);
    }

    public List<Image> searchImagesByNumber(int number)
    {
        return _imagesRepository.GetImagesByPhotoNumber(number);
    }
}