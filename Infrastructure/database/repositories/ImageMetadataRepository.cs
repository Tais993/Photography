using Application.interfaces.infrastructure;
using Application.interfaces.infrastructure.repositories;
using Domain.entities;
using Microsoft.Extensions.Logging;
using static Infrastructure.database.repositories.DatabaseMappers;

namespace Infrastructure.database.repositories;

public class ImageMetadataRepository : IImageMetadataRepository
{
    private readonly RepositoryHelper _db;
    private readonly ILogger<ImageMetadataRepository> _logger;

    public ImageMetadataRepository(
        ILogger<ImageMetadataRepository> logger,
        RepositoryHelper db)
    {
        _logger = logger;
        _db = db;
    }

    // Probably shouldn't use this method
    public List<ImageMetadata> GetAll()
    {
        _logger.LogDebug("Getting all image metadata");

        List<ImageMetadata> imageMetadata = _db.QueryMultiple("""
                                                              SELECT
                                                                  image_id,
                                                                  im.metadata_key,
                                                                  metadata_value,
                                                                  metadata_type,
                                                                  display_name,
                                                                  description
                                                              FROM public.image_metadata im
                                                              JOIN public.metadata m on m.metadata_key = im.metadata_key
                                                              """, MapImageMetadata);

        _logger.LogDebug("Found {Count} image metadata records", imageMetadata.Count);

        return imageMetadata;
    }

    public void Insert(ImageMetadata entity)
    {
        _logger.LogDebug("Inserting image metadata, image: {ImageId}, metadata key: {MetadataKey}", entity.ImageId, entity.MetadataKey);

        _db.Execute("""
                    INSERT INTO public.image_metadata(image_id, metadata_key, metadata_value) 
                    VALUES ($1, $2, $3) 
                    """, entity.ImageId, entity.MetadataKey, entity.MetadataValue);
    }

    public void Update(ImageMetadata entity)
    {
        _logger.LogDebug("Updating image metadata, image: {ImageId}, metadata key: {MetadataKey}", entity.ImageId, entity.MetadataKey);

        _db.Execute("""
                    UPDATE public.image_metadata
                    SET metadata_value = $1
                    WHERE image_id = $2
                    AND metadata_key = $3;
                    """, entity.MetadataValue, entity.ImageId, entity.MetadataKey);
    }

    public ImageMetadata? GetByKey(int imageId, string metadataKey)
    {
        _logger.LogDebug("Getting image metadata, image: {ImageId}, metadata key: {MetadataKey}", imageId, metadataKey);

        return _db.QueryOrDefault("""
                                  SELECT
                                      image_id,
                                      metadata_value,
                                      metadata_type,
                                      m.metadata_key,
                                      display_name,
                                      description
                                  FROM public.image_metadata im
                                  JOIN public.metadata m on m.metadata_key = im.metadata_key
                                  WHERE image_id = $1 AND m.metadata_key = $2
                                  """, MapImageMetadata, imageId, metadataKey);
    }

    public void DeleteByKey(int imageId, string metadataKey)
    {
        _logger.LogDebug("Deleting image metadata, image: {ImageId}, metadata key: {MetadataKey}", imageId, metadataKey);

        _db.Execute("""
                    DELETE FROM public.image_metadata
                    WHERE image_id = $1 
                    AND metadata_key = $2
                    """, imageId, metadataKey);
    }

    public List<ImageMetadata> GetAllByImageId(int imageId)
    {
        _logger.LogDebug("Getting metadata for image: {ImageId}", imageId);

        List<ImageMetadata> imageMetadata = _db.QueryMultiple("""
                                                              SELECT
                                                                  image_id,
                                                                  metadata_value,
                                                                  m.metadata_key,
                                                                  metadata_type,
                                                                  display_name,
                                                                  description
                                                              FROM public.image_metadata im
                                                              JOIN public.metadata m on m.metadata_key = im.metadata_key
                                                              WHERE image_id = $1
                                                              """, MapImageMetadata, imageId);

        _logger.LogDebug("Found {Count} metadata records for image: {ImageId}", imageMetadata.Count, imageId);

        return imageMetadata;
    }
    
}