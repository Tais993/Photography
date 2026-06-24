using Domain.entities;

namespace Application.interfaces.infrastructure;

public interface IImageMetadataRepository
{
    public List<ImageMetadata> GetAll();
    public void Insert(ImageMetadata imageMetadata);
    public void Update(ImageMetadata imageMetadata);
    public ImageMetadata? GetByKey(int imageId, string metadataKey);
    public void DeleteByKey(int imageId, string metadataKey);
    public List<ImageMetadata> GetAllByImageId(int imageId);
}