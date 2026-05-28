using Domain.entities;

namespace Application.services.interfaces;

/// <summary>
///     The project services handles with everything project related.
///     Initializing and resolving the most predominant functions.
/// </summary>
public interface IProjectService
{
    /// <summary>
    ///     Based on a given directory, resolves any projects that can be found within the main, and or its parent folder.
    ///     If none is found, it'll return "0".
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public int ResolveProjectId(string directory);

    /// <summary>
    ///     This resolves the given directory, but only if the given project ID is 0.
    ///     Based on a given directory, resolves any projects that can be found within the main, and or its parent folder.
    ///     If none is found, it'll return "0".
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="possiblyEmptyProjectId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public int ResolveProjectId(string directory, int possiblyEmptyProjectId);

    /// <summary>
    ///     Based on a given directory, resolves any projects that can be found within the main, and or its parent folder.
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Project? ResolveProject(string directory);

    /// <summary>
    ///     Based on a given directory, resolves any projects that can be found within the main, and or its parent folder.
    ///     If none is found, it'll return "null".
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="possibleEmptyProjectId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Project? ResolveProject(string directory, int possibleEmptyProjectId);


    /// <summary>
    ///     Returns the given image based on its ID.
    /// </summary>
    /// <param name="imageId"></param>
    /// <returns></returns>
    public Image GetImageById(int imageId);
    
    /// <summary>
    ///     Returns the amount of images that are connected to the project
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public int GetProjectImageCount(int projectId);
    
    /// <summary>
    ///     Returns the given project based on its ID.
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public Project GetProjectById(int projectId);
    
    /// <summary>
    ///     Returns a list of all existing projects, this doesnt look at categories yet.
    /// </summary>
    /// <returns></returns>
    public List<Project> GetAllProjects();
    
    
    /// <summary>
    ///     Initialises the given projects directory into the database,
    ///     additionally this adds a file to the filesystem to remember the projects ID.
    ///     Any images from within the project will also be loaded in with its metadata saved into the database.
    ///     Any collection folders are ignored for now, additionally if a subfolder was given its ignored as its not recognized
    ///     as a project folder.
    ///     In future versions this method will run recursively, including for any sub/collection folders.
    /// </summary>
    /// <param name="projectDirectory"></param>
    public void InitialiseExistingFolder(string projectDirectory);
}