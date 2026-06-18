using Application.interfaces.infrastructure;
using Domain.entities;
using Microsoft.Extensions.Logging;
using static Infrastructure.database.repositories.DatabaseMappers;

namespace Infrastructure.database.repositories;

public class ProjectMetadataRepository : IProjectMetadataRepository
{
    private readonly RepositoryHelper _db;
    private readonly ILogger<ProjectMetadataRepository> _logger;

    public ProjectMetadataRepository(
        ILogger<ProjectMetadataRepository> logger,
        RepositoryHelper db)
    {
        _logger = logger;
        _db = db;
    }

    // Probably shouldn't use this method
    public List<ProjectMetadata> GetAll()
    {
        _logger.LogDebug("Getting all project metadata");

        List<ProjectMetadata> projectMetadata = _db.QueryMultiple("""
                                                                  SELECT
                                                                  project_id,
                                                                  m.metadata_key,
                                                                  metadata_value,
                                                                  metadata_type,
                                                                  display_name,
                                                                  description
                                                                  FROM public.project_metadata pm
                                                                  JOIN public.metadata m on m.metadata_key = pm.metadata_key
                                                                  """, MapProjectMetadata);

        _logger.LogDebug("Found {Count} project metadata records", projectMetadata.Count);

        return projectMetadata;
    }

    public void Insert(ProjectMetadata entity)
    {
        _logger.LogDebug("Inserting project metadata, project: {ProjectId}, metadata key: {MetadataKey}", entity.ProjectId, entity.MetadataKey);

        _db.Execute("""
                    INSERT INTO public.project_metadata(project_id, metadata_key, metadata_value) 
                    VALUES ($1, $2, $3) 
                    """, entity.ProjectId, entity.MetadataKey, entity.MetadataValue);
    }

    public void Update(ProjectMetadata entity)
    {
        _logger.LogDebug("Updating project metadata, project: {ProjectId}, metadata key: {MetadataKey}", entity.ProjectId, entity.MetadataKey);

        _db.Execute("""
                    UPDATE public.project_metadata
                    SET metadata_value = $1
                    WHERE project_id = $2
                    AND metadata_key = $3;
                    """, entity.MetadataValue, entity.ProjectId, entity.MetadataKey);
    }

    public ProjectMetadata? GetByKey(int projectId, string metadataKey)
    {
        _logger.LogDebug("Getting project metadata, project: {ProjectId}, metadata key: {MetadataKey}", projectId, metadataKey);

        return _db.Query("""
                         SELECT
                             project_id,
                             metadata_value,
                             metadata_type,
                             m.metadata_key,
                             display_name,
                             description
                         FROM public.project_metadata pm
                         JOIN public.metadata m on m.metadata_key = pm.metadata_key
                         WHERE project_id = $1 AND m.metadata_key = $2
                         """, MapProjectMetadata, projectId, metadataKey);
    }

    public void DeleteByKey(int projectId, string metadataKey)
    {
        _logger.LogDebug("Deleting project metadata, project: {ProjectId}, metadata key: {MetadataKey}", projectId, metadataKey);

        _db.Execute("""
                    DELETE FROM project_metadata
                           WHERE project_id = $1 
                           AND metadata_key = $2
                    """, projectId, metadataKey);
    }

    public List<ProjectMetadata> GetAllByProjectId(int projectId)
    {
        _logger.LogDebug("Getting metadata for project: {ProjectId}", projectId);

        List<ProjectMetadata> projectMetadata = _db.QueryMultiple("""
                                                                  SELECT
                                                                      project_id,
                                                                      metadata_value,
                                                                      m.metadata_key,
                                                                      metadata_type,
                                                                      display_name,
                                                                      description
                                                                  FROM public.project_metadata pm
                                                                  JOIN public.metadata m on m.metadata_key = pm.metadata_key
                                                                  WHERE project_id = $1
                                                                  """, MapProjectMetadata, projectId);

        _logger.LogDebug("Found {Count} metadata records for project: {ProjectId}", projectMetadata.Count, projectId);

        return projectMetadata;
    }
}