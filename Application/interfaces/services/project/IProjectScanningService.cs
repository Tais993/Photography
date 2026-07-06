using Domain.entities;

namespace Application.interfaces.services.project;

public interface IProjectScanningService
{
    /// <summary>
    /// Scans a given project, this is automatically recursive and also checks all subprojects
    /// </summary>
    /// <param name="project"></param>
    /// <param name="recursive">whenever to check all subprojects</param>
    /// <param name="checkExistingImages">Only check filesystem or also verify with DB for old/deleted images</param>
    public void ScanProject(Project project, bool recursive = true, bool checkExistingImages = false);
}