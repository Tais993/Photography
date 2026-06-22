using Domain.entities;

namespace Application.interfaces.services;

public interface IProjectResolverService
{
    /// <summary>
    ///     This resolves the given directory, but only if the given project ID is 0.
    ///     Based on a given directory, resolves any projects that can be found within the main, and or its parent folder.
    ///     If none is found, it'll return "0".
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="possiblyEmptyProjectId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public int ResolveProjectId(string directory, int possiblyEmptyProjectId = 0);

    /// <summary>
    ///     Based on a given directory, resolves any projects that can be found within the main, and or its parent folder.
    ///     If none is found, it'll return "null".
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="possibleEmptyProjectId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Project? ResolveProject(string directory, int possibleEmptyProjectId = 0);
}