using Application.interfaces;
using Domain.entities;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure.database.repositories;

public class MetadataRepository : IMetadataRepository
{
    private readonly RepositoryHelper _db;
    private readonly ILogger<MetadataRepository> _logger;

    public MetadataRepository(NpgsqlDataSource dataSource,
        ILogger<MetadataRepository> logger,
        RepositoryHelper db)
    {
        _logger = logger;
        _db = db;
    }

    public Metadata? GetByKey(string key)
    {
        return _db.Query("""
                         SELECT metadata_key, metadata_type, display_name, description
                         FROM public.metadata
                         WHERE metadata_key = $1
                         """, MapMetadata, key);
    }

    public List<Metadata> GetAll()
    {
        return _db.QueryMultiple("""
                                 SELECT metadata_key, metadata_type, display_name, description
                                 FROM public.metadata
                                 """, MapMetadata);
    }

    public Metadata Insert(Metadata entity)
    {
        return _db.Query("""
                         INSERT INTO public.metadata(metadata_key, metadata_type, display_name, description) 
                         VALUES ($1, $2, $3, $4)
                         RETURNING metadata_key, metadata_type, display_name, description
                         """, MapMetadata, entity.MetadataKey, entity.MetadataType, entity.DisplayName,
            entity.Description);
    }

    public void Update(Metadata entity)
    {
        if (entity.MetadataKey is null)
        {
            throw new ArgumentException("Metadata must have an ID", nameof(entity));
        }

        _db.Execute("""
                    UPDATE public.metadata
                    SET metadata_type = $1,
                        display_name = $2,
                        description = $3
                    WHERE metadata_key = $4
                    """, entity.MetadataType, entity.DisplayName, entity.Description, entity.MetadataKey);
    }

    public void DeleteById(string metadataKey)
    {
        _db.Execute("""
                    DELETE FROM public.metadata
                    WHERE metadata_key = $1
                    """, metadataKey);
    }

    private static Metadata MapMetadata(NpgsqlDataReader reader)
    {
        return new Metadata(
            (string)reader["metadata_key"],
            (string)reader["metadata_type"],
            (string)reader["display_name"],
            (string)reader["description"]
        );
    }
}