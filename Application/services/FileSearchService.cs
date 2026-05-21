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

    public List<Image> SearchImagesByNameOrNumber(string name)
    {
        if (int.TryParse(name, out _))
        {
            return _imagesRepository.GetImagesByPhotoNumber(name);
        }

        return _imagesRepository.GetImagesByFileName(name);
    }

    public List<Image> SearchImagesByNameOrNumber(int projectId, string name)
    {
        if (int.TryParse(name, out _))
        {
            return _imagesRepository.GetImagesByPhotoNumber(projectId, name);
        }


        return _imagesRepository.GetImagesByFileName(projectId, name);
    }

    public List<Image> SearchImages(FileSearchSettings fileSearchSettings)
    {
        if (fileSearchSettings.FileNameOrNumber != null)
        {
            if (int.TryParse(fileSearchSettings.FileNameOrNumber, out _))
            {
                fileSearchSettings.FileNumber = fileSearchSettings.FileNameOrNumber;
            }
            else
            {
                fileSearchSettings.FileName = fileSearchSettings.FileNameOrNumber;
            }
        }
        
        return _imagesRepository.SearchImages(fileSearchSettings);
    }
}