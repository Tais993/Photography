using Npgsql;
using PhotographyNET.database.entities;

namespace PhotographyNET.database.repositories;

public class MetadataRepository : AbstractIdRepository<Metadata>
{
    public MetadataRepository(NpgsqlDataSource dataSource, ILogger<MetadataRepository> logger) : base(dataSource, logger)
    {
    }

    public override Metadata? GetByKey(int id)
    {
        return QuerySingle("""
                           SELECT id, metadata_key, metadata_type, display_name, description
                           FROM public.metadata
                           WHERE id = $1
                           """, MapMetadata, id);
    }

    public override List<Metadata> GetAll()
    {
        return QueryMultiple("""
                             SELECT id, metadata_key, metadata_type, display_name, description
                             FROM public.metadata
                             """, MapMetadata);
    }

    public override Metadata Insert(Metadata entity)
    {
        return QuerySingle("""
                           INSERT INTO public.metadata(metadata_key, metadata_type, display_name, description) 
                           VALUES ($1, $2, $3, $4)
                           RETURNING id, metadata_key, metadata_type, display_name, description
                           """, MapMetadata, entity.MetadataKey, entity.MetadataType, entity.DisplayName,
                   entity.Description)
               ?? throw new Exception("Insert failed");
    }

    public override void Update(Metadata entity)
    {
        if (entity?.Id is null) throw new Exception("yeah i need actual stuff");

        Execute("""
                UPDATE public.metadata
                SET metadata_key = $1,
                    metadata_type = $2,
                    display_name = $3,
                    description = $4
                WHERE id = $5
                """, entity.MetadataKey, entity.MetadataType, entity.DisplayName, entity.Description, entity.Id);
    }

    public override void DeleteByKey(int id)
    {
        Execute("""
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