using Application.interfaces;
using Application.services.interfaces;
using Domain.entities;

namespace Application.services;

public class ProjectMetadataService : IProjectMetadataService
{
    private readonly IMetadataRepository _metadataRepository;
    private readonly IProjectMetadataRepository _projectMetadataRepository;

    public ProjectMetadataService(IProjectMetadataRepository projectMetadataRepository,
        IMetadataRepository metadataRepository)
    {
        _projectMetadataRepository = projectMetadataRepository;
        _metadataRepository = metadataRepository;
    }

    public Metadata CreateMetadata(string metadataKey, string metadataType, string displayName, string description)
    {
        return CreateMetadata(new Metadata(metadataKey, metadataType, displayName, description));
    }

    public Metadata CreateMetadata(Metadata metadata)
    {
        return _metadataRepository.Insert(metadata);
    }

    public void UpdateMetadata(string metadataKey, string metadataType, string displayName, string description)
    {
        UpdateMetadata(new Metadata(metadataKey, metadataType, displayName, description));
    }

    public void UpdateMetadata(Metadata metadata)
    {
        _metadataRepository.Update(metadata);
    }

    public Metadata? GetMetadata(Metadata metadata)
    {
        if (metadata.MetadataKey == null)
        {
            throw new ArgumentNullException("metadata.MetadataKey");
        }

        return GetMetadata(metadata.MetadataKey);
    }

    public Metadata? GetMetadata(string metadataKey)
    {
        return _metadataRepository.GetByKey(metadataKey);
    }

    public Metadata? GetMetadata(int projectId)
    {
        return _metadataRepository.GetById(projectId);
    }


    public void DeleteMetadata(Metadata metadata)
    {
        if (metadata.MetadataKey == null)
        {
            throw new ArgumentNullException("metadata.MetadataKey");
        }

        DeleteMetadata(metadata.MetadataKey);
    }


    public void DeleteMetadata(string metadataKey)
    {
        _metadataRepository.DeleteById(metadataKey);
    }


    public List<ProjectMetadata> GetProjectMetadata(Project project)
    {
        if (project.Id == null)
        {
            throw new ArgumentNullException("project.Id");
        }

        return GetProjectMetadata((int)project.Id);
    }

    public List<ProjectMetadata> GetProjectMetadata(int projectId)
    {
        return _projectMetadataRepository.GetAllByProjectId(projectId);
    }

    public void AddMetadataToProject(int projectId, string metadataKey, string? value)
    {
        value ??= string.Empty;

        ProjectMetadata projectMetadata = new ProjectMetadata(projectId, metadataKey, value);

        _projectMetadataRepository.Insert(projectMetadata);
    }
    

    public void RemoveMetadataFromProject(int projectId, string metadataKey)
    {
        _projectMetadataRepository.DeleteByKey(projectId, metadataKey);
    }
}