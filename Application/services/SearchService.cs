using Application.services.interfaces;
using Domain.entities;
using Infrastructure.database.repositories;
using Microsoft.Extensions.Logging;

namespace Application.services;

public class SearchService : ISearchService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IImageRepository _imagesRepository;
    private readonly ILogger<SearchService> _logger;

    public SearchService(IImageRepository imagesRepository, IProjectRepository projectRepository, ILogger<SearchService> logger)
    {
        _imagesRepository = imagesRepository;
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public List<Image> SearchImages(ImageSearchSettings imageSearchSettings)
    {
        imageSearchSettings.FileNameOrNumber = ValidateStringValue(imageSearchSettings.FileNameOrNumber);
        imageSearchSettings.FileName = ValidateStringValue(imageSearchSettings.FileName);
        imageSearchSettings.FileNumber = ValidateStringValue(imageSearchSettings.FileNumber);
        imageSearchSettings.FolderName = ValidateStringValue(imageSearchSettings.FolderName);
        imageSearchSettings.FileType = ValidateStringValue(imageSearchSettings.FileType);
        
        if (imageSearchSettings.FileNameOrNumber != null)
        {
            if (int.TryParse(imageSearchSettings.FileNameOrNumber, out _))
            {
                imageSearchSettings.FileNumber = imageSearchSettings.FileNameOrNumber;
            }
            else
            {
                imageSearchSettings.FileName = imageSearchSettings.FileNameOrNumber;
            }
        }
        
        return _imagesRepository.SearchImages(imageSearchSettings);
    }

    public List<Project> SearchProjects(ProjectSearchSettings projectSearchSettings)
    {
        projectSearchSettings.ProjectName = ValidateStringValue(projectSearchSettings.ProjectName);
        projectSearchSettings.ProjectPath = ValidateStringValue(projectSearchSettings.ProjectPath);
        
        return _projectRepository.SearchProjects(projectSearchSettings);
    }

    private static string? ValidateStringValue(string? value)
    {
        if (value == null)
        {
            return null;
        }
        
        value = value.Trim();

        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value;
    }
}