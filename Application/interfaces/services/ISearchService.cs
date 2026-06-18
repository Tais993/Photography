using Domain.entities;
using Domain.entities.search;

namespace Application.interfaces.services;

public interface ISearchService
{
    /// <summary>
    ///    Based on the given settings, searches all image with those requirements
    /// </summary>
    /// <param name="imageSearchSettings">The settings for the search</param>
    /// <returns></returns>
    public PaginatedResult<Image> SearchImages(ImageSearchSettings imageSearchSettings);

    /// <summary>
    ///     Based on the given settings, searches all projects with those requirements
    /// </summary>
    /// <param name="projectSearchSettings">The settings for the search</param>
    /// <returns></returns>
    public PaginatedResult<Project> SearchProjects(ProjectSearchSettings projectSearchSettings);
}