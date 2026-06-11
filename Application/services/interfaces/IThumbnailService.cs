using Domain.entities;

namespace Application.services.interfaces;

public interface IThumbnailService
{
    ThumbnailResult GetThumbnail(int imageId, string size = "default");
}