using Application.interfaces.infrastructure;
using Application.interfaces.infrastructure.services;
using Application.interfaces.services;
using Application.interfaces.services.project;
using Microsoft.Extensions.Logging;
using static Application.Constants;

namespace Application.services;

public class ApplicationResetService : IApplicationResetService
{
    private readonly IProjectService _projectService;
    private readonly IDatabaseResetService _databaseResetService;
    private readonly IFiles _files;
    private readonly ILogger<ApplicationResetService> _logger;

    public ApplicationResetService(IProjectService projectService, IFiles files, ILogger<ApplicationResetService> logger, IDatabaseResetService databaseResetService)
    {
        _projectService = projectService;
        _files = files;
        _logger = logger;
        _databaseResetService = databaseResetService;
    }

    public void ResetApplicationData(bool dryRun = false)
    {
        _logger.LogWarning("Resetting application data");
        RemoveProjectInfoFiles(dryRun);
        DropDatabase(dryRun);
        _logger.LogWarning("Application data reset");
    }

    public void RemoveProjectInfoFiles(bool dryRun)
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

    public void DropDatabase(bool dryRun)
    {
        if (dryRun) return;
        
        _databaseResetService.DropDatabase();
    }
}