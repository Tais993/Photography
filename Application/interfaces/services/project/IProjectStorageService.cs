using Domain.entities;

namespace Application.interfaces.services.project;

public interface IProjectStorageService
{
    public Project UpdateStorageInfo(Project project);
    public void UpdateStorageInfo(int projectId);
}