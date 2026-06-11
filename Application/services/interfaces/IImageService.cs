using Domain.entities;

namespace Application.services;

public interface IImageService
{
    /// <summary>
    ///     Returns the given image based on its ID.
    /// </summary>
    /// <param name="imageId"></param>
    /// <returns></returns>
    public Image GetImageById(int imageId);
    
    /// <summary>
    ///     Returns all images that are linked to the project
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    List<Image> GetImagesByProjectId(int projectId);
    
    /// <summary>
    ///     Returns the amount of images that are connected to the project
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public int GetProjectImageCount(int projectId);
    
    /// <summary>
    ///     Based on the given list, filters all RAW files that ALSO have a JPG in the list
    /// </summary>
    /// <param name="images"></param>
    /// <returns></returns>
    IEnumerable<Image> HideRawFilesWhenNonRawExists(IEnumerable<Image> images);
}