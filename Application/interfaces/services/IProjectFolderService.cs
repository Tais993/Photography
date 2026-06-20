using Domain.entities;

namespace Application.interfaces.services;

public interface IProjectFolderService
{
    ProjectFolder GetRequiredFolder(int projectId, ProjectFolderRole role);
    ProjectFolder? GetFolder(int projectId, ProjectFolderRole role);
    
    /// <summary>
    /// Checks or the given destination folder can be mapped to a required folder.
    /// Otherwise, it just returns itself.
    /// </summary>
    /// <param name="project"></param>
    /// <param name="destinationFolder"></param>
    /// <returns></returns>
    string ResolveFolder(Project project, string destinationFolder);
    
    string GetRequiredFolderPath(int projectId, ProjectFolderRole role);
    string GetRequiredFolderName(int projectId, ProjectFolderRole role);
}