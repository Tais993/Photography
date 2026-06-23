using Application.interfaces.infrastructure;
using Application.interfaces.services.project;
using Domain.entities;
using Microsoft.Extensions.Logging;

namespace Application.services.project;

public class ProjectResolverService : IProjectResolverService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectInfoFileService _projectInfoFileService;
    private readonly ILogger<ProjectResolverService> _logger;


    public ProjectResolverService(
        IProjectRepository projectRepository,
        IProjectInfoFileService projectInfoFileService,
        ILogger<ProjectResolverService> logger)
    {
        _projectRepository = projectRepository;
        _projectInfoFileService = projectInfoFileService;
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

        int? projectId = _projectInfoFileService.ResolveProjectId(directory);

        if (projectId is null)
        {
            _logger.LogWarning("Project info file was not found for directory: {Directory}", directory);
            return 0;
        }

        _logger.LogDebug("Resolved project id: {ProjectId}", projectId);

        return projectId.Value;
    }

    public Project? ResolveProject(string directory, int possibleEmptyProjectId = 0)
    {
        int projectId = ResolveProjectId(directory, possibleEmptyProjectId);
        _logger.LogDebug("Resolving project: {ProjectId}", projectId);

        if (projectId == 0) return null;

        Project? project = _projectRepository.GetById(projectId);

        if (project is null)
        {
            _logger.LogWarning("Project info file points to a project that does not exist: {ProjectId}", projectId);
        }

        return project;
    }
}