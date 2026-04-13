using Npgsql;
using PhotographyNET.database.entities;
using PhotographyNET.database.repositories.interfaces;

namespace PhotographyNET.database.repositories;

public class ProjectMetadataRepository : AbstractRepository<ProjectMetadata, ProjectMetadataIds>,
    IKeyRepository<ProjectMetadata, ProjectMetadataIds>
{
    public ProjectMetadataRepository(NpgsqlDataSource dataSource, ILogger<ProjectMetadataRepository> logger) : base(dataSource, logger)
     {
    }

    public override List<ProjectMetadata> GetAll()
    {
        return QueryMultiple("""
                             SELECT
                                 project_id,
                                 metadata_id,
                                 metadata_value
                             FROM public.project_metadata
                             """, MapProjectMetadata);
    }

    public override ProjectMetadata Insert(ProjectMetadata entity)
    {
        return QuerySingle("""
                           INSERT INTO public.project_metadata(project_id, metadata_id, metadata_value) 
                           VALUES ($1, $2, $3) 
                           """, MapProjectMetadata) ?? throw new InvalidOperationException();
    }

    public override void Update(ProjectMetadata entity)
    {
        Execute("""
                UPDATE public.project_metadata
                SET metadata_value = $1
                WHERE project_id = $2
                AND metadata_id = $3;
                """, entity.MetadataValue, entity.ProjectId, entity.MetadataId);
    }

    public override ProjectMetadata? GetByKey(ProjectMetadataIds key)
    {
        return QuerySingle("""
                           SELECT
                               project_id,
                               metadata_id,
                               metadata_value
                           FROM project_metadata
                           WHERE project_id = $1 
                           AND metadata_id = $2
                           """, MapProjectMetadata, key.ProjectId, key.MetadataId);
    }

    public override void DeleteByKey(ProjectMetadataIds key)
    {
        {
            Execute("""
                               DELETE FROM project_metadata
                                      WHERE project_id = $1 
                                      AND metadata_id = $2
                               """, key.ProjectId, key.MetadataId);
        }
    }

    public List<ProjectMetadata> GetAllByProjectId(int projectId)
    {
        return QueryMultiple("""
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