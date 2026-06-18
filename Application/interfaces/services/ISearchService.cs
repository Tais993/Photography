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
    ///     Based on the given list, filters all RAW files that ALSO have a JPG in the list
    /// </summary>
    /// <param name="images"></param>
    /// <returns></returns>
    IEnumerable<Image> HideRawFilesWhenNonRawExists(IEnumerable<Image> images);
    
    /// <summary>
    ///     Based on the given settings, searches all projects with those requirements
    /// </summary>
    /// <param name="projectSearchSettings">The settings for the search</param>
    /// <returns></returns>
    public List<Project> SearchProjects(ProjectSearchSettings projectSearchSettings);
}