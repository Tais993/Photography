using Application.interfaces.infrastructure;
using Application.interfaces.infrastructure.repositories;
using Application.interfaces.services.metadata;
using Domain.entities;
using Microsoft.Extensions.Logging;

namespace Application.services.metadata;

public class ProjectMetadataService : IProjectMetadataService
{
    private readonly IProjectMetadataRepository _projectMetadataRepository;
    private readonly ILogger<ProjectMetadataService> _logger;

    public ProjectMetadataService(
        IProjectMetadataRepository projectMetadataRepository,
        ILogger<ProjectMetadataService> logger)
    {
        _projectMetadataRepository = projectMetadataRepository;
        _logger = logger;
    }

    public List<ProjectMetadata> GetProjectMetadata(Project project)
    {
        if (project.Id == null)
        {
            _logger.LogWarning("Could not get project metadata because project id was null");
            throw new ArgumentNullException("project.Id");
        }

        return GetProjectMetadata((int)project.Id);
    }

    public List<ProjectMetadata> GetProjectMetadata(int projectId)
    {
        _logger.LogDebug("Getting metadata for project: {ProjectId}", projectId);

        List<ProjectMetadata> metadata = _projectMetadataRepository.GetAllByProjectId(projectId);

        _logger.LogDebug("Found {Count} metadata values for project: {ProjectId}", metadata.Count, projectId);
        return metadata;
    }

    public ProjectMetadata? GetProjectMetadata(Project project, string metadataKey)
    {
        if (project.Id == null)
        {
            _logger.LogWarning("Could not get project metadata because project id was null");
            throw new ArgumentNullException("project.Id");
        }

        return GetProjectMetadata((int)project.Id, metadataKey);
    }

    public ProjectMetadata? GetProjectMetadata(int projectId, string metadataKey)
    {
        _logger.LogDebug("Getting metadata for project: {ProjectId}, and key {MetadataKey}", projectId, metadataKey);

        return _projectMetadataRepository.GetByKey(projectId, metadataKey);
    }

    public void AddMetadataToProject(int projectId, string metadataKey, string? value)
    {
        value ??= string.Empty;

        _logger.LogInformation("Adding metadata to project: {ProjectId}, metadata key: {MetadataKey}", projectId, metadataKey);

        ProjectMetadata projectMetadata = new ProjectMetadata(projectId, metadataKey, value);

        _projectMetadataRepository.Insert(projectMetadata);
    }

    public void RemoveMetadataFromProject(int projectId, string metadataKey)
    {
        _logger.LogInformation("Removing metadata from project: {ProjectId}, metadata key: {MetadataKey}", projectId, metadataKey);
        _projectMetadataRepository.DeleteByKey(projectId, metadataKey);
    }
}