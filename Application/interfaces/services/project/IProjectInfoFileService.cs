using Domain.entities;

namespace Application.interfaces.services.project;

public interface IProjectInfoFileService
{
    string GetProjectInfoPath(string projectPath);
    bool HasProjectInfoFile(string projectPath);

    /// <summary>
    ///     Only verifies the given directory for a project-ionfo file
    /// </summary>
    /// <param name="projectPath"></param>
    /// <returns></returns>
    int? ReadProjectId(string projectPath);
    
    /// <summary>
    ///     This also goes through all parent folders till it finds a project-info file.
    ///     If it finds one, it returns the directory of the project-info file.
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    string? FindProjectDirectory(string directory);

    /// <summary>
    ///     This goes through all parent folders, till a folder is recognised as a project directory.
    ///     If it finds one, it will return the project-info's ID
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    int? ResolveProjectId(string directory);
    
    void WriteProjectInfoFile(Project project);
}