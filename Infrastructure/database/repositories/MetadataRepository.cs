using Application.interfaces.infrastructure;
using Domain.entities;
using Microsoft.Extensions.Logging;
using static Infrastructure.database.repositories.DatabaseMappers;

namespace Infrastructure.database.repositories;

public class MetadataRepository : IMetadataRepository
{
    private readonly RepositoryHelper _db;
    private readonly ILogger<MetadataRepository> _logger;

    public MetadataRepository(
        ILogger<MetadataRepository> logger,
        RepositoryHelper db)
    {
        _logger = logger;
        _db = db;
    }

    public Metadata? GetByKey(string key)
    {
        _logger.LogDebug("Getting metadata by key: {MetadataKey}", key);

        return _db.Query("""
                         SELECT metadata_key, metadata_type, display_name, description
                         FROM public.metadata
                         WHERE metadata_key = $1
                         """, MapMetadata, key);
    }

    public List<Metadata> GetAll()
    {
        _logger.LogDebug("Getting all metadata");

        List<Metadata> metadata = _db.QueryMultiple("""
                                                    SELECT metadata_key, metadata_type, display_name, description
                                                    FROM public.metadata
                                                    """, MapMetadata);

        _logger.LogDebug("Found {Count} metadata records", metadata.Count);

        return metadata;
    }

    public Metadata Insert(Metadata entity)
    {
        _logger.LogDebug("Inserting metadata: {MetadataKey}", entity.MetadataKey);

        Metadata metadata = _db.Query("""
                                      INSERT INTO public.metadata(metadata_key, metadata_type, display_name, description) 
                                      VALUES ($1, $2, $3, $4)
                                      RETURNING metadata_key, metadata_type, display_name, description
                                      """, MapMetadata, entity.MetadataKey, entity.MetadataType, entity.DisplayName,
            entity.Description);

        _logger.LogDebug("Inserted metadata: {MetadataKey}", metadata.MetadataKey);

        return metadata;
    }

    public void Update(Metadata entity)
    {
        if (entity.MetadataKey is null)
        {
            _logger.LogWarning("Could not update metadata because metadata key was null");
            throw new ArgumentException("Metadata must have an ID", nameof(entity));
        }

        _logger.LogDebug("Updating metadata: {MetadataKey}", entity.MetadataKey);

        _db.Execute("""
                    UPDATE public.metadata
                    SET metadata_type = $1,
                        display_name = $2,
                        description = $3
                    WHERE metadata_key = $4
                    """, entity.MetadataType, entity.DisplayName, entity.Description, entity.MetadataKey);
    }

    public void DeleteByKey(string metadataKey)
    {
        _logger.LogDebug("Deleting metadata: {MetadataKey}", metadataKey);

        _db.Execute("""
                    DELETE FROM public.metadata
                    WHERE metadata_key = $1
                    """, metadataKey);
    }
}