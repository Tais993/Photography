using Application.interfaces.infrastructure;
using Application.interfaces.services.project;
using Domain.entities;
using Microsoft.Extensions.Logging;
using static Application.Constants;

namespace Application.services.project;

public class ProjectInfoFileService : IProjectInfoFileService
{
    private readonly IFiles _files;
    private readonly ILogger<ProjectInfoFileService> _logger;

    public ProjectInfoFileService(IFiles files, ILogger<ProjectInfoFileService> logger)
    {
        _files = files;
        _logger = logger;
    }

    public string GetProjectInfoPath(string projectPath)
    {
        return _files.Combine(projectPath, ProjectInfoFile);
    }

    public bool HasProjectInfoFile(string projectPath)
    {
        return _files.Exists(GetProjectInfoPath(projectPath));
    }

    public int? ReadProjectId(string projectPath)
    {
        string projectInfoPath = GetProjectInfoPath(projectPath);

        if (!_files.Exists(projectInfoPath))
        {
            return null;
        }

        string content = _files.ReadAllText(projectInfoPath);

        if (!int.TryParse(content.Trim(), out int projectId))
        {
            _logger.LogWarning("Project info file did not contain a valid project id: {ProjectInfoPath}", projectInfoPath);
            return null;
        }

        return projectId;
    }

    public string? FindProjectDirectory(string directory)
    {
        string? currentDirectory = _files.GetFullPath(directory);

        while (currentDirectory is not null)
        {
            if (HasProjectInfoFile(currentDirectory))
            {
                return currentDirectory;
            }

            DirectoryInfo? parentDirectory = _files.GetParentDirectory(currentDirectory);
            currentDirectory = parentDirectory?.FullName;
        }

        return null;
    }

    public int? ResolveProjectId(string directory)
    {
        string? projectDirectory = FindProjectDirectory(directory);

        if (projectDirectory is null)
        {
            return null;
        }

        return ReadProjectId(projectDirectory);
    }

    public void WriteProjectInfoFile(Project project)
    {
        if (project.Id is null)
        {
            throw new ArgumentNullException(nameof(project.Id));
        }

        string projectInfoPath = GetProjectInfoPath(project.Path);
        _files.WriteAllText(project.Id.Value.ToString(), projectInfoPath);
        
        _logger.LogInformation("Wrote project info file: {ProjectInfoPath}", projectInfoPath);
    }
}