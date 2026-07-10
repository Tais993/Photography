using Domain.entities;

namespace Application.services.project;

public interface IProjectUpdateService
{
    void UpdateProjectByPath(string projectPath);
    void UpdateProject(Project? project, string? projectPath = null);
}