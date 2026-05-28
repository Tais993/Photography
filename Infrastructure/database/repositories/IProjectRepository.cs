using Domain.entities;

namespace Infrastructure.database.repositories;

public interface IProjectRepository
{
    Project GetById(int id);
    List<Project> GetAll();
    Project Insert(Project project);
    void Update(Project project);
    void DeleteById(int id);

    int GetProjectImageCount(int projectId);
    
    public List<Project> SearchProjects(ProjectSearchSettings projectSearchSettings);
}