using Application.services.interfaces;
using Domain.entities;
using Infrastructure.database.repositories;

namespace Application.services;

public class FileSearchService : IFileSearchService
{
    private readonly IImageRepository _imagesRepository;

    public FileSearchService(IImageRepository imagesRepository)
    {
        _imagesRepository = imagesRepository;
    }

    public List<Image> SearchImagesByNameOrNumber(int projectId, string name)
    {
        if (int.TryParse(name, out _)) return SearchImagesByNumber(projectId, name);

        return SearchImagesByName(projectId, name);
    }

    public List<Image> SearchImagesByNameOrNumber(string name)
    {
        if (int.TryParse(name, out _)) return SearchImagesByNumber(name);

        return SearchImagesByName(name);
    }

    public List<Image> SearchImagesByName(string name)
    {
        return _imagesRepository.GetImagesByFileName(name);
    }

    public List<Image> SearchImagesByName(int projectId, string name)
    {
        return _imagesRepository.GetImagesByFileName(projectId, name);
    }

    public List<Image> SearchImagesByNumber(string number)
    {
        return _imagesRepository.GetImagesByPhotoNumber(number);
    }

    public List<Image> SearchImagesByNumber(int projectId, string number)
    {
        return _imagesRepository.GetImagesByPhotoNumber(projectId, number);
    }
}