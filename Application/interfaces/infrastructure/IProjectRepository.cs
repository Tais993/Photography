using Domain.entities;
using Domain.entities.search;

namespace Application.interfaces;

public interface IProjectRepository
{
    Project? GetById(int id);
    List<Project> GetAll();
    Project Insert(Project project);
    void Update(Project project);
    void DeleteById(int id);
    
    int GetProjectCount();
    
    public List<Project> SearchProjects(ProjectSearchSettings projectSearchSettings);
}