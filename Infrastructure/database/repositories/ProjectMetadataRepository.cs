using Domain.entities;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure.database.repositories;

public class ProjectMetadataRepository
{
    private readonly RepositoryHelper _db;
    private readonly ILogger<ProjectMetadataRepository> _logger;

    public ProjectMetadataRepository(NpgsqlDataSource dataSource,
        ILogger<ProjectMetadataRepository> logger,
        RepositoryHelper db)
    {
        _logger = logger;
        _db = db;
    }

    // PRobably shouldn't use this method
    private List<ProjectMetadata> GetAll()
    {
        return _db.QueryMultiple("""
                                 SELECT
                                 project_id,
                                 metadata_id,
                                 metadata_value,
                                 metadata_key,
                                 metadata_type,
                                 display_name,
                                 description
                                 FROM public.project_metadata pm
                                 JOIN public.metadata m on m.id = pm.metadata_id
                                 """, MapProjectMetadata);
    }

    public void Insert(ProjectMetadata entity)
    {
        _db.Execute("""
                    INSERT INTO public.project_metadata(project_id, metadata_id, metadata_value) 
                    VALUES ($1, $2, $3) 
                    """, entity.ProjectId, entity.MetadataId, entity.MetadataValue);
    }

    public void Update(ProjectMetadata entity)
    {
        _db.Execute("""
                    UPDATE public.project_metadata
                    SET metadata_value = $1
                    WHERE project_id = $2
                    AND metadata_id = $3;
                    """, entity.MetadataValue, entity.ProjectId, entity.MetadataId);
    }

    public ProjectMetadata? GetById(int projectId, int metadataId)
    {
        return _db.Query("""
                         SELECT
                             project_id,
                             metadata_id,
                             metadata_value,
                             metadata_key,
                             metadata_type,
                             display_name,
                             description
                         FROM public.project_metadata pm
                         JOIN public.metadata m on m.id = pm.metadata_id
                         WHERE project_id = $1 AND metadata_id = $2
                         """, MapProjectMetadata, projectId, metadataId);
    }

    public void DeleteByKey(int projectId, int metadataId)
    {
        {
            _db.Execute("""
                        DELETE FROM project_metadata
                               WHERE project_id = $1 
                               AND metadata_id = $2
                        """, projectId, metadataId);
        }
    }

    public List<ProjectMetadata> GetAllByProjectId(int projectId)
    {
        return _db.QueryMultiple("""
                                 SELECT
                                     project_id,
                                     metadata_id,
                                     metadata_value,
                                     metadata_key,
                                     metadata_type,
                                     display_name,
                                     description
                                 FROM public.project_metadata pm
                                 JOIN public.metadata m on m.id = pm.metadata_id
                                 WHERE project_id = $1
                                 """, MapProjectMetadata, projectId);
    }

    private static ProjectMetadata MapProjectMetadata(NpgsqlDataReader reader)
    {
        return new ProjectMetadata(
            (int)reader["project_id"],
            (int)reader["metadata_id"],
            (string)reader["metadata_value"],
            (string)reader["metadata_key"],
            (string)reader["metadata_type"],
            (string)reader["display_name"],
            (string)reader["description"]
        );
    }
}