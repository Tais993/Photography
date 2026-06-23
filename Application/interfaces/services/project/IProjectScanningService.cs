using Domain.entities;

namespace Application.interfaces.services.project;

public interface IProjectScanningService
{    
    public void ScanProject(Project project);
    public void ScanProjectSubFolders(string projectDirectory, int projectId);
    
    
    /// <summary>
    /// This method expects a project's subfolder already.
    /// </summary>
    /// <param name="projectDirectory">The project root directory.</param>
    /// <param name="projectSubDirectory">A subfolder from within a project that contains images.</param>
    /// <param name="projectId">The project id.</param>
    public void ScanProjectSubFolder(string projectDirectory, string projectSubDirectory, int projectId);
}