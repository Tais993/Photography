using Npgsql;
using PhotographyNET.database.entities;

namespace PhotographyNET.database.repositories;

public class ProjectMetadataRepository
{
    private RepositoryHelper _db;
    private ILogger<ProjectMetadataRepository> _logger;

    public ProjectMetadataRepository(NpgsqlDataSource dataSource,
        ILogger<ProjectMetadataRepository> logger,
        RepositoryHelper db)
    {
        this._logger = logger;
        this._db = db;
    }

    public List<ProjectMetadata> GetAll()
    {
        return _db.QueryMultiple("""
                             SELECT
                                 project_id,
                                 metadata_id,
                                 metadata_value
                             FROM public.project_metadata
                             """, MapProjectMetadata);
    }

    public ProjectMetadata Insert(ProjectMetadata entity)
    {
        return _db.Query("""
                           INSERT INTO public.project_metadata(project_id, metadata_id, metadata_value) 
                           VALUES ($1, $2, $3) 
                           """, MapProjectMetadata) ?? throw new InvalidOperationException();
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

    public ProjectMetadata? GetByKey(ProjectMetadataIds key)
    {
        return _db.Query("""
                           SELECT
                               project_id,
                               metadata_id,
                               metadata_value
                           FROM project_metadata
                           WHERE project_id = $1 
                           AND metadata_id = $2
                           """, MapProjectMetadata, key.ProjectId, key.MetadataId);
    }

    public void DeleteByKey(ProjectMetadataIds key)
    {
        {
            _db.Execute("""
                               DELETE FROM project_metadata
                                      WHERE project_id = $1 
                                      AND metadata_id = $2
                               """, key.ProjectId, key.MetadataId);
        }
    }

    public List<ProjectMetadata> GetAllByProjectId(int projectId)
    {
        return _db.QueryMultiple("""
                             SELECT
                                 project_id,
                                 metadata_id,
                                 metadata_value
                             FROM public.project_metadata
                             WHERE project_id = $1
                             """, MapProjectMetadata, projectId);
    }

    private static ProjectMetadata MapProjectMetadata(NpgsqlDataReader reader)
    {
        return new ProjectMetadata(
            (int)reader["project_id"],
            (int)reader["metadata_id"],
            (string)reader["metadata_value"]
        );
    }
}