using Domain.entities;

namespace Application.interfaces.services.metadata;

public interface IImageMetadataService
{
    public List<ImageMetadata> GetImageMetadata(Image image);
    public List<ImageMetadata> GetImageMetadata(int imageId);

    public ImageMetadata? GetImageMetadata(Image image, string metadataKey);
    public ImageMetadata? GetImageMetadata(int imageId, string metadataKey);

    public void AddMetadataToImage(Image image, string metadataKey, string? value);
    public void AddMetadataToImage(int imageId, string metadataKey, string? value);

    public void UpdateMetadataForImage(Image image, string metadataKey, string? value);
    public void UpdateMetadataForImage(int imageId, string metadataKey, string? value);

    public void RemoveMetadataFromImage(Image image, string metadataKey);
    public void RemoveMetadataFromImage(int imageId, string metadataKey);
}