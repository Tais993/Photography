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

    public List<Image> searchImagesByNameOrNumber(string name)
    {
        if (int.TryParse(name, out _))
        {
            return searchImagesByNumber(name);
        }
        else
        {
            return searchImagesByName(name);
        }
    }

    public List<Image> searchImagesByName(string name)
    {
        return _imagesRepository.GetImagesByFileName(name);
    }

    public List<Image> searchImagesByNumber(string number)
    {
        return _imagesRepository.GetImagesByPhotoNumber(number);
    }
}