using Domain.entities;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure.database.repositories;

public class ProjectMetadataRepository : IProjectMetadataRepository
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
    public List<ProjectMetadata> GetAll()
    {
        return _db.QueryMultiple("""
                                 SELECT
                                 project_id,
                                 metadata_key,
                                 metadata_value,
                                 metadata_type,
                                 display_name,
                                 description
                                 FROM public.project_metadata pm
                                 JOIN public.metadata m on m.metadata_key = pm.metadata_key
                                 """, MapProjectMetadata);
    }

    public void Insert(ProjectMetadata entity)
    {
        _db.Execute("""
                    INSERT INTO public.project_metadata(project_id, metadata_key, metadata_value) 
                    VALUES ($1, $2, $3) 
                    """, entity.ProjectId, entity.MetadataKey, entity.MetadataValue);
    }

    public void Update(ProjectMetadata entity)
    {
        _db.Execute("""
                    UPDATE public.project_metadata
                    SET metadata_value = $1
                    WHERE project_id = $2
                    AND metadata_key = $3;
                    """, entity.MetadataValue, entity.ProjectId, entity.MetadataKey);
    }

    public ProjectMetadata? GetByKey(int projectId, string metadataKey)
    {
        return _db.Query("""
                         SELECT
                             project_id,
                             metadata_value,
                             metadata_key,
                             metadata_type,
                             display_name,
                             description
                         FROM public.project_metadata pm
                         JOIN public.metadata m on m.metadata_key = pm.metadata_key
                         WHERE project_id = $1 AND metadata_key = $2
                         """, MapProjectMetadata, projectId, metadataKey);
    }

    public void DeleteByKey(int projectId, string metadataKey)
    {
        {
            _db.Execute("""
                        DELETE FROM project_metadata
                               WHERE project_id = $1 
                               AND metadata_key = $2
                        """, projectId, metadataKey);
        }
    }

    public List<ProjectMetadata> GetAllByProjectId(int projectId)
    {
        return _db.QueryMultiple("""
                                 SELECT
                                     project_id,
                                     metadata_value,
                                     metadata_key,
                                     metadata_type,
                                     display_name,
                                     description
                                 FROM public.project_metadata pm
                                 JOIN public.metadata m on m.metadata_key = pm.metadata_key
                                 WHERE project_id = $1
                                 """, MapProjectMetadata, projectId);
    }

    private static ProjectMetadata MapProjectMetadata(NpgsqlDataReader reader)
    {
        return new ProjectMetadata(
            (int)reader["project_id"],
            (string)reader["metadata_value"],
            (string)reader["metadata_key"],
            (string)reader["metadata_type"],
            (string)reader["display_name"],
            (string)reader["description"]
        );
    }
}