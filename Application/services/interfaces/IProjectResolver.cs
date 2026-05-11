using Domain.entities;

namespace Application.services.interfaces;

public interface IProjectResolver
{
    /// <summary>
    /// Based on the given directory, and projects at that directory are searched for and returned.
    /// </summary>
    /// <param name="directory">The directory for which to look </param>
    /// <returns></returns>
    public Project resolveProject(string directory);

    /// <summary>
    /// Based on the given directory, looks for any projects and or its subprojects to load their info into the database.
    /// The images will also be stored in the database for searching, an uninitialized project has no use or functionalities.
    /// </summary>
    /// <param name="projectDirectory"></param>
    public void initialiseExistingFolder(string projectDirectory);
}