using Application.interfaces.infrastructure;
using Application.interfaces.infrastructure.repositories;
using Application.interfaces.services.project;
using Domain.entities;

namespace Application.services.project;

public class ProjectStorageService : IProjectStorageService
{
    private readonly IProjectRepository _repository;
    private readonly IFiles _files;

    public ProjectStorageService(IFiles files, IProjectRepository repository)
    {
        _files = files;
        _repository = repository;
    }

    public Project UpdateStorageInfo(Project project)
    {
        string projectPath = project.Path;

        project.StorageTotalBytes = _files.GetFolderSize(projectPath);
        project.StorageLocalBytes = _files.GetLocalFolderSize(projectPath);
        project.StorageLastCalculated = DateTime.Now;
        
        _repository.UpdateStorage(project);
        
        return project;
    }

    public void UpdateStorageInfo(int projectId)
    {
        throw new NotImplementedException();
    }
}