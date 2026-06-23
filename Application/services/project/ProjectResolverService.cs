using Application.interfaces.infrastructure;
using Application.interfaces.services.project;
using Domain.entities;
using Microsoft.Extensions.Logging;

namespace Application.services.project;

public class ProjectResolverService : IProjectResolverService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<ProjectService> _logger;
    private readonly IFiles _files;


    public ProjectResolverService(IProjectRepository projectRepository, IFiles files,
        ILogger<ProjectService> logger)
    {
        _projectRepository = projectRepository;
        _files = files;
        _logger = logger;
    }

    public int ResolveProjectId(string directory, int possibleEmptyProjectId = 0)
    {
        if (possibleEmptyProjectId != 0)
        {
            _logger.LogInformation("Project ID was provided by user: {Id}", possibleEmptyProjectId);
            return possibleEmptyProjectId;
        }

        _logger.LogDebug("Resolving project id for directory: {Directory}", directory);

        string? projectInfoPath = ResolveProjectInfoPath(directory);

        if (projectInfoPath is null)
        {
            _logger.LogWarning("Project info file was not found for directory: {Directory}", directory);
            return 0;
        }

        string projectInfoContent = _files.ReadAllText(projectInfoPath);

        if (!int.TryParse(projectInfoContent, out int projectId))
        {
            _logger.LogWarning(
                "Project info file does not contain a valid project id: {ProjectInfoPath}",
                projectInfoPath);

            return 0;
        }

        _logger.LogDebug(
            "Project info file found: {ProjectInfoPath}, id: {Id}",
            projectInfoPath,
            projectId);

        return projectId;
    }

    public Project? ResolveProject(string directory, int possibleEmptyProjectId = 0)
    {
        int projectId = ResolveProjectId(directory, possibleEmptyProjectId);
        _logger.LogDebug("Resolving project: {ProjectId}", projectId);

        if (projectId == 0) return null;
        return _projectRepository.GetById(projectId);
    }

    /// <summary>
    ///     To figure out the project path, it will go through the current and all parent folders till it finds one.
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    private string? ResolveProjectInfoPath(string directory)
    {
        string? currentDirectory = _files.GetFullPath(directory);

        while (currentDirectory is not null)
        {
            string projectInfoPath = _files.Combine(
                currentDirectory,
                Constants.ProjectInfoFile);

            if (_files.Exists(projectInfoPath))
            {
                return projectInfoPath;
            }

            DirectoryInfo? parentDirectory = _files.GetParentDirectory(currentDirectory);
            if (parentDirectory is null) return null;

            currentDirectory = parentDirectory?.FullName;
        }

        return null;
    }
}