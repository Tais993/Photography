using Domain.entities;

namespace Application.services.interfaces;

public interface IThumbnailService
{
    
    /// <summary>
    ///  
    /// </summary>
    /// <param name="imageId"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    ThumbnailResult GetThumbnail(int imageId, string size = "default");
}