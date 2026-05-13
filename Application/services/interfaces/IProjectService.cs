using Domain.entities;

namespace Application.services.interfaces;

/// <summary>
/// The project services handles with everything project related.
/// Initializing and resolving the most predominant functions.
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Based on a given directory, resolves any projects that can be found within the main, and or its parent folder.
    /// If none is found, it'll return "0".
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public int ResolveProjectId(string directory);

    /// <summary>
    /// Based on a given directory, resolves any projects that can be found within the main, and or its parent folder.
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Project ResolveProject(string directory);

    /// <summary>
    /// Initialises the given projects directory into the database,
    /// additionally this adds a file to the filesystem to remember the projects ID.
    /// Any images from within the project will also be loaded in with its metadata saved into the database.
    ///
    /// Any collection folders are ignored for now, additionally if a subfolder was given its ignored as its not recognized as a project folder.
    ///
    /// In future versions this method will run recursively, including for any sub/collection folders.
    /// </summary>
    /// <param name="projectDirectory"></param>
    public void InitialiseExistingFolder(string projectDirectory);
}