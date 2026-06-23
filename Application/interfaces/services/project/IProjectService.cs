using Domain.entities;

namespace Application.interfaces.services.project;

/// <summary>
///     The project services handles with everything project related.
///     Initializing and resolving the most predominant functions.
/// </summary>
public interface IProjectService
{
    
    /// <summary>
    ///     Returns the number of projects that exist
    /// </summary>
    /// <returns></returns>
    public int GetProjectCount();
    
    /// <summary>
    ///     Creates a project based on the given name and date.
    ///     This creates the initial folder in the default project folder, and ensures all required folders exist too.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    public Project CreateProject(string name, DateOnly date);
    
    /// <summary>
    ///     Returns the given project based on its ID.
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public Project? GetProjectById(int projectId);
    
    /// <summary>
    ///     Returns a list of all existing projects, this doesn't look at categories yet.
    /// </summary>
    /// <returns></returns>
    public List<Project> GetAllProjects();
}