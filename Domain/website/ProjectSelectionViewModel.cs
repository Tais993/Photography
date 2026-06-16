using Domain.entities;

namespace Domain.website;

public class ProjectIndexViewModel
{
    public List<Project> Projects = [];
    public Project? SelectedProject;
    public int? SelectedProjectId;
    public int ProjectCount;
}
