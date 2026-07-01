using Application.interfaces.infrastructure.repositories;
using Application.interfaces.services;
using Domain.entities;
using Domain.entities.search;
using Microsoft.Extensions.Logging;

namespace Application.services;

public class SearchService : ISearchService
{
    private const int DefaultPageNumber = 1;
    private const int DefaultPageSize = 24;
    private const int MaxPageSize = 200;

    private readonly ISearchRepository _searchRepository;
    private readonly ILogger<SearchService> _logger;

    public SearchService(ISearchRepository searchRepository, ILogger<SearchService> logger)
    {
        _searchRepository = searchRepository;
        _logger = logger;
    }

    public PaginatedResult<Image> SearchImages(ImageSearchSettings imageSearchSettings)
    {
        _logger.LogDebug("Searching for images");

        imageSearchSettings.FileNameOrNumber = ValidateStringValue(imageSearchSettings.FileNameOrNumber);
        imageSearchSettings.FileName = ValidateStringValue(imageSearchSettings.FileName);
        imageSearchSettings.FileNumber = ValidateStringValue(imageSearchSettings.FileNumber);
        imageSearchSettings.FolderName = ValidateStringValue(imageSearchSettings.FolderName);
        imageSearchSettings.FileType = ValidateStringValue(imageSearchSettings.FileType);

        ValidatePagination(imageSearchSettings);
        ApplyFileNameOrNumberSearch(imageSearchSettings);

        int totalItems = _searchRepository.CountImages(imageSearchSettings);

        if (PageNumberIsAboveTotalPages(imageSearchSettings, totalItems))
        {
            imageSearchSettings.PageNumber = GetLastPageNumber(totalItems, imageSearchSettings.PageSize);
        }

        List<Image> images = _searchRepository.SearchImages(imageSearchSettings);

        _logger.LogInformation("Found {Count} images while searching", totalItems);

        return new PaginatedResult<Image>
        {
            Items = images,
            PageNumber = imageSearchSettings.PageNumber,
            PageSize = imageSearchSettings.PageSize,
            TotalItems = totalItems
        };
    }

    public PaginatedResult<Project> SearchProjects(ProjectSearchSettings projectSearchSettings)
    {
        _logger.LogDebug("Searching for projects");

        projectSearchSettings.ProjectName = ValidateStringValue(projectSearchSettings.ProjectName);
        projectSearchSettings.ProjectPath = ValidateStringValue(projectSearchSettings.ProjectPath);

        ValidatePagination(projectSearchSettings);

        int totalItems = _searchRepository.CountProjects(projectSearchSettings);

        if (PageNumberIsAboveTotalPages(projectSearchSettings, totalItems))
        {
            projectSearchSettings.PageNumber = GetLastPageNumber(totalItems, projectSearchSettings.PageSize);
        }

        List<Project> projects = _searchRepository.SearchProjects(projectSearchSettings);

        _logger.LogInformation("Found {Count} projects while searching", totalItems);

        return new PaginatedResult<Project>
        {
            Items = projects,
            PageNumber = projectSearchSettings.PageNumber,
            PageSize = projectSearchSettings.PageSize,
            TotalItems = totalItems
        };
    }

    private void ApplyFileNameOrNumberSearch(ImageSearchSettings imageSearchSettings)
    {
        if (imageSearchSettings.FileNameOrNumber == null)
        {
            return;
        }

        if (int.TryParse(imageSearchSettings.FileNameOrNumber, out _))
        {
            imageSearchSettings.FileNumber = imageSearchSettings.FileNameOrNumber;

            _logger.LogDebug(
                "Searching images by file number: {FileNumber}",
                imageSearchSettings.FileNumber);
        }
        else
        {
            imageSearchSettings.FileName = imageSearchSettings.FileNameOrNumber;

            _logger.LogDebug(
                "Searching images by file name: {FileName}",
                imageSearchSettings.FileName);
        }
    }

    private static void ValidatePagination(SearchSettings searchSettings)
    {
        if (searchSettings.PageNumber < 1)
        {
            searchSettings.PageNumber = DefaultPageNumber;
        }

        if (searchSettings.PageSize < 1)
        {
            searchSettings.PageSize = DefaultPageSize;
        }

        if (searchSettings.PageSize > MaxPageSize)
        {
            searchSettings.PageSize = MaxPageSize;
        }
    }

    private static bool PageNumberIsAboveTotalPages(SearchSettings searchSettings, int totalItems)
    {
        if (totalItems == 0)
        {
            return false;
        }

        int totalPages = GetLastPageNumber(totalItems, searchSettings.PageSize);

        return searchSettings.PageNumber > totalPages;
    }

    private static int GetLastPageNumber(int totalItems, int pageSize)
    {
        return (int)Math.Ceiling(totalItems / (double)pageSize);
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