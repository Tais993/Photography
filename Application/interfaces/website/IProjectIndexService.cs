using Domain.entities;
using Domain.website;

namespace Application.interfaces.website;

public interface IProjectIndexService
{
    ProjectIndexViewModel GetProjectIndex(ProjectIndexRequest request);
    ProjectViewModel CreateProjectView(Project project, int? selectedProjectId);
    SelectedProjectViewModel CreateSelectedProjectView(Project project);
    void OpenProjectFolder(int projectId);
    void OpenProjectCommandLine(int projectId);
}