using Domain.entities;

namespace Application.services.interfaces;

public interface IFileSearchService
{
    
    /// <summary>
    /// Based on the given value, looks the image up by its number or by the name
    /// </summary>
    /// <param name="name">The value to search for</param>
    /// <returns></returns>
    public List<Image> searchImagesByNameOrNumber(string name);
    
    /// <summary>
    /// Based on the given name, all images with the same name will be searched for.
    /// </summary>
    /// <param name="name">The file's name to look for</param>
    /// <returns></returns>
    public List<Image> searchImagesByName(string name);
    
    /// <summary>
    /// Based on the given number, all images with the same number will be searched for.
    /// </summary>
    /// <param name="number">The file's number to look for</param>
    /// <returns></returns>
    public List<Image> searchImagesByNumber(string number);
}