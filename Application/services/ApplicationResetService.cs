using Application.interfaces.infrastructure;
using Application.interfaces.infrastructure.services;
using Application.interfaces.services;
using Application.interfaces.services.project;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static Application.Constants;

namespace Application.services;

public class ApplicationResetService : IApplicationResetService
{
    private readonly IProjectService _projectService;
    private readonly IDatabaseResetService _databaseResetService;
    private readonly IConfiguration _configuration;
    private readonly IFiles _files;
    private readonly ILogger<ApplicationResetService> _logger;

    public ApplicationResetService(IProjectService projectService, IFiles files, ILogger<ApplicationResetService> logger,
        IDatabaseResetService databaseResetService, IConfiguration configuration)
    {
        _projectService = projectService;
        _files = files;
        _logger = logger;
        _databaseResetService = databaseResetService;
        _configuration = configuration;
    }

    public void ResetApplicationData(bool dryRun = false)
    {
        _logger.LogWarning("Resetting application data");
        RemoveProjectInfoFiles(dryRun);
        ResetCache(dryRun);
        DropDatabase(dryRun);
        _logger.LogWarning("Application data reset");
    }

    private void RemoveProjectInfoFiles(bool dryRun)
    {
        _projectService.GetAllProjects().ForEach(project =>
        {
            // creating logging for project name, event date and ID
            _logger.LogInformation("Removing project info file for project: {}", project.Name);

            string projectInfoPath = _files.Combine(project.Path, ProjectInfoFile);


            if (!_files.Exists(projectInfoPath))
            {
                _logger.LogWarning("Project info file does not exist: {ProjectInfoPath}", projectInfoPath);
                return;
            }

            if (dryRun)
            {
                return;
            }

            _files.DeleteFile(projectInfoPath);
        });
    }

    private void ResetCache(bool dryRun)
    {
        string cacheRoot = _configuration.GetValue<string>(ConfigThumbnailsCachePath) ??
                           _files.Combine(AppContext.BaseDirectory, "thumbnail-cache");

        if (!_files.Exists(cacheRoot))
        {
            _logger.LogWarning("Cache folder does not exist: {CacheRoot}", cacheRoot);
        }
        else if (!dryRun)
        {
            if (!dryRun)
            {
                _files.FolderDelete(cacheRoot, true);
                _logger.LogInformation("Deleted cache folder: {CacheRoot}", cacheRoot);
            }
        }
        else
        {
            _logger.LogInformation("Cache folder does exist: {CacheRoot}", cacheRoot);
            _logger.LogInformation("Dry run, not deleting cache folder: {CacheRoot}", cacheRoot);
        }
    }

    private void DropDatabase(bool dryRun)
    {
        if (dryRun) return;

        _databaseResetService.DropDatabase();
    }
}