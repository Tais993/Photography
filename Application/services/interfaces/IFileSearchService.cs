using Domain.entities;

namespace Application.services.interfaces;

public interface IFileSearchService
{
    /// <summary>
    ///     Based on the given value, looks the image up by its number or by the name
    /// </summary>
    /// <param name="name">The value to search for</param>
    /// <returns></returns>
    public List<Image> SearchImagesByNameOrNumber(string name);

    /// <summary>
    ///     Based on the given value, looks the image up by its number or by the name
    /// </summary>
    /// <param name="projectId">The ID of the project to look through</param>
    /// <param name="name">The value to search for</param>
    /// <returns></returns>
    public List<Image> SearchImagesByNameOrNumber(int projectId, string name);
    
    /// <summary>
    ///    Based on the given settings, searches all image with those requirements
    /// </summary>
    /// <param name="fileSearchSettings">The settings for the search</param>
    /// <returns></returns>
    public List<Image> SearchImages(FileSearchSettings fileSearchSettings);
}