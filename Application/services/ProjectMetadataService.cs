using Application.interfaces;
using Application.services.interfaces;
using Domain.entities;
using Microsoft.Extensions.Logging;

namespace Application.services;

public class ProjectMetadataService : IProjectMetadataService
{
    private readonly IMetadataRepository _metadataRepository;
    private readonly IProjectMetadataRepository _projectMetadataRepository;
    private readonly ILogger<ProjectMetadataService> _logger;

    public ProjectMetadataService(
        IProjectMetadataRepository projectMetadataRepository,
        IMetadataRepository metadataRepository,
        ILogger<ProjectMetadataService> logger)
    {
        _projectMetadataRepository = projectMetadataRepository;
        _metadataRepository = metadataRepository;
        _logger = logger;
    }

    public Metadata CreateMetadata(string metadataKey, string metadataType, string displayName, string description)
    {
        return CreateMetadata(new Metadata(metadataKey, metadataType, displayName, description));
    }

    public Metadata CreateMetadata(Metadata metadata)
    {
        _logger.LogInformation("Creating metadata: {MetadataKey}", metadata.MetadataKey);
        return _metadataRepository.Insert(metadata);
    }

    public void UpdateMetadata(string metadataKey, string metadataType, string displayName, string description)
    {
        UpdateMetadata(new Metadata(metadataKey, metadataType, displayName, description));
    }

    public void UpdateMetadata(Metadata metadata)
    {
        _logger.LogInformation("Updating metadata: {MetadataKey}", metadata.MetadataKey);
        _metadataRepository.Update(metadata);
    }

    public Metadata? GetMetadata(Metadata metadata)
    {
        if (metadata.MetadataKey == null)
        {
            _logger.LogWarning("Could not get metadata because metadata key was null");
            throw new ArgumentNullException("metadata.MetadataKey");
        }

        return GetMetadata(metadata.MetadataKey);
    }

    public Metadata? GetMetadata(string metadataKey)
    {
        _logger.LogDebug("Getting metadata: {MetadataKey}", metadataKey);
        return _metadataRepository.GetByKey(metadataKey);
    }

    public void DeleteMetadata(Metadata metadata)
    {
        if (metadata.MetadataKey == null)
        {
            _logger.LogWarning("Could not delete metadata because metadata key was null");
            throw new ArgumentNullException("metadata.MetadataKey");
        }

        DeleteMetadata(metadata.MetadataKey);
    }


    public void DeleteMetadata(string metadataKey)
    {
        _logger.LogInformation("Deleting metadata: {MetadataKey}", metadataKey);
        _metadataRepository.DeleteByKey(metadataKey);
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