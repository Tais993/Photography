using Application.services.interfaces;
using Domain.entities;
using Infrastructure.database.repositories;

namespace Application.services;

public class ProjectMetadataService : IProjectMetadataService
{
    private readonly MetadataRepository _metadataRepository;
    private readonly ProjectMetadataRepository _projectMetadataRepository;

    public ProjectMetadataService(ProjectMetadataRepository projectMetadataRepository,
        MetadataRepository metadataRepository)
    {
        _projectMetadataRepository = projectMetadataRepository;
        _metadataRepository = metadataRepository;
    }

    public Metadata CreateMetadata(string metadataKey, string metadataType, string displayName, string description)
    {
        return CreateMetadata(new Metadata(0, metadataKey, metadataType, displayName, description));
    }

    public Metadata CreateMetadata(Metadata metadata)
    {
        return _metadataRepository.Insert(metadata);
    }

    public Metadata? GetMetadata(Metadata metadata)
    {
        if (metadata.Id == null)
        {
            throw new ArgumentNullException("metadata.Id");
        }

        return GetMetadata((int)metadata.Id);
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
        if (metadata.Id == null)
        {
            throw new ArgumentNullException("metadata.Id");
        }

        DeleteMetadata((int)metadata.Id);
    }


    public void DeleteMetadata(int projectId)
    {
        _metadataRepository.DeleteById(projectId);
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

    public void AddMetadataToProject(int projectId, int metadataId, string? value)
    {
        value ??= string.Empty;

        ProjectMetadata projectMetadata = new ProjectMetadata(projectId, metadataId, value);

        _projectMetadataRepository.Insert(projectMetadata);
    }

    public void DeleteMetadataFromProject(int projectId, int metadataId)
    {
        _projectMetadataRepository.DeleteByKey(projectId, metadataId);
    }
}