using Application.interfaces.infrastructure.repositories;
using Application.interfaces.services.metadata;
using Domain.entities;
using Microsoft.Extensions.Logging;

namespace Application.services.metadata;

public class ImageMetadataService : IImageMetadataService
{
    private readonly IImageMetadataRepository _imageMetadataRepository;
    private readonly ILogger<ImageMetadataService> _logger;

    public ImageMetadataService(
        IImageMetadataRepository imageMetadataRepository,
        ILogger<ImageMetadataService> logger)
    {
        _imageMetadataRepository = imageMetadataRepository;
        _logger = logger;
    }

    public List<ImageMetadata> GetImageMetadata(Image image)
    {
        if (image.Id == null)
        {
            _logger.LogWarning("Could not get image metadata because image id was null");
            throw new ArgumentNullException("image.Id");
        }

        return GetImageMetadata((int)image.Id);
    }

    public List<ImageMetadata> GetImageMetadata(int imageId)
    {
        _logger.LogDebug("Getting metadata for image: {ImageId}", imageId);

        List<ImageMetadata> metadata = _imageMetadataRepository.GetAllByImageId(imageId);

        _logger.LogDebug("Found {Count} metadata values for image: {ImageId}", metadata.Count, imageId);
        return metadata;
    }

    public ImageMetadata? GetImageMetadata(Image image, string metadataKey)
    {
        if (image.Id == null)
        {
            _logger.LogWarning("Could not get image metadata because image id was null");
            throw new ArgumentNullException("image.Id");
        }

        return GetImageMetadata((int)image.Id, metadataKey);
    }

    public ImageMetadata? GetImageMetadata(int imageId, string metadataKey)
    {
        _logger.LogDebug("Getting metadata for image: {ImageId}, metadata key: {MetadataKey}", imageId, metadataKey);

        return _imageMetadataRepository.GetByKey(imageId, metadataKey);
    }

    public void AddMetadataToImage(Image image, string metadataKey, string? value)
    {
        if (image.Id == null)
        {
            _logger.LogWarning("Could not add metadata to image because image id was null");
            throw new ArgumentNullException("image.Id");
        }

        AddMetadataToImage((int)image.Id, metadataKey, value);
    }

    public void AddMetadataToImage(int imageId, string metadataKey, string? value)
    {
        value ??= string.Empty;

        _logger.LogInformation("Adding metadata to image: {ImageId}, metadata key: {MetadataKey}", imageId, metadataKey);

        ImageMetadata imageMetadata = new ImageMetadata(imageId, metadataKey, value);

        _imageMetadataRepository.Insert(imageMetadata);
    }

    public void UpdateMetadataForImage(Image image, string metadataKey, string? value)
    {
        if (image.Id == null)
        {
            _logger.LogWarning("Could not update image metadata because image id was null");
            throw new ArgumentNullException("image.Id");
        }

        UpdateMetadataForImage((int)image.Id, metadataKey, value);
    }

    public void UpdateMetadataForImage(int imageId, string metadataKey, string? value)
    {
        value ??= string.Empty;

        _logger.LogInformation("Updating metadata for image: {ImageId}, metadata key: {MetadataKey}", imageId, metadataKey);

        ImageMetadata imageMetadata = new ImageMetadata(imageId, metadataKey, value);

        _imageMetadataRepository.Update(imageMetadata);
    }

    public void RemoveMetadataFromImage(Image image, string metadataKey)
    {
        if (image.Id == null)
        {
            _logger.LogWarning("Could not remove image metadata because image id was null");
            throw new ArgumentNullException("image.Id");
        }

        RemoveMetadataFromImage((int)image.Id, metadataKey);
    }

    public void RemoveMetadataFromImage(int imageId, string metadataKey)
    {
        _logger.LogInformation("Removing metadata from image: {ImageId}, metadata key: {MetadataKey}", imageId, metadataKey);
        _imageMetadataRepository.DeleteByKey(imageId, metadataKey);
    }
}