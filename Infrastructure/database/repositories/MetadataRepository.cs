using Domain.entities;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure.database.repositories;

public class MetadataRepository
{
    private RepositoryHelper _db;
    private ILogger<MetadataRepository> _logger;

    public MetadataRepository(NpgsqlDataSource dataSource,
        ILogger<MetadataRepository> logger,
        RepositoryHelper db)
    {
        this._logger = logger;
        this._db = db;
    }

    public Metadata? GetByKey(int id)
    {
        return _db.Query("""
                           SELECT id, metadata_key, metadata_type, display_name, description
                           FROM public.metadata
                           WHERE id = $1
                           """, MapMetadata, id);
    }

    public List<Metadata> GetAll()
    {
        return _db.QueryMultiple("""
                             SELECT id, metadata_key, metadata_type, display_name, description
                             FROM public.metadata
                             """, MapMetadata);
    }

    public Metadata Insert(Metadata entity)
    {
        return _db.Query("""
                         INSERT INTO public.metadata(metadata_key, metadata_type, display_name, description) 
                         VALUES ($1, $2, $3, $4)
                         RETURNING id, metadata_key, metadata_type, display_name, description
                         """, MapMetadata, entity.MetadataKey, entity.MetadataType, entity.DisplayName,
            entity.Description);
    }

    public void Update(Metadata entity)
    {
        if (entity?.Id is null) throw new ArgumentException("Metadata must have an ID", nameof(entity));

        _db.Execute("""
                UPDATE public.metadata
                SET metadata_key = $1,
                    metadata_type = $2,
                    display_name = $3,
                    description = $4
                WHERE id = $5
                """, entity.MetadataKey, entity.MetadataType, entity.DisplayName, entity.Description, entity.Id);
    }

    public void DeleteByKey(int id)
    {
        _db.Execute("""
                           DELETE FROM public.metadata
                           WHERE id = $1
                           """, id);
    }

    private static Metadata MapMetadata(NpgsqlDataReader reader)
    {
        return new Metadata(
            (int)reader["id"],
            (string)reader["metadata_key"],
            (string)reader["metadata_type"],
            (string)reader["display_name"],
            (string)reader["description"]
        );
    }
}