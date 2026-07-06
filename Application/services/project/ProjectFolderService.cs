using Application.interfaces.infrastructure;
using Application.interfaces.infrastructure.repositories;
using Application.interfaces.services.metadata;
using Application.interfaces.services.project;
using Domain.entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static Application.Constants;

namespace Application.services.project;

public class ProjectFolderService : IProjectFolderService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMetadataService _projectMetadataService;
    private readonly ILogger<ProjectFolderService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IFiles _files;

    public ProjectFolderService(IProjectRepository projectRepository, IProjectMetadataService projectMetadataService,
        IFiles files, ILogger<ProjectFolderService> logger, IConfiguration configuration)
    {
        _projectRepository = projectRepository;
        _projectMetadataService = projectMetadataService;
        _files = files;
        _logger = logger;
        _configuration = configuration;
    }

    public ProjectFolder? GetFolder(int projectId, ProjectFolderRole role)
    {
        Project project = _projectRepository.GetById(projectId);
        string metadataKey = ToMetadataKey(role);

        ProjectMetadata? folderMetadata = _projectMetadataService.GetProjectMetadata(projectId, metadataKey);

        if (folderMetadata is null || string.IsNullOrWhiteSpace(folderMetadata.MetadataValue))
        {
            _logger.LogDebug("No folder metadata found for project {ProjectId}, role {FolderRole}, metadata key {MetadataKey}",
                projectId, role, metadataKey);

            return null;
        }

        string folderName = folderMetadata.MetadataValue;
        string absolutePath = _files.Combine(project.Path, folderName);
        bool exists = _files.Exists(absolutePath);

        return new ProjectFolder(
            role,
            metadataKey,
            folderName,
            folderName,
            absolutePath,
            exists);
    }

    public ProjectFolder GetRequiredFolder(int projectId, ProjectFolderRole role)
    {
        ProjectFolder? folder = GetFolder(projectId, role);

        if (folder is not null)
        {
            return folder;
        }

        _logger.LogWarning("Required folder metadata is missing for project {ProjectId}, role {FolderRole}", projectId, role);

        throw new InvalidOperationException(
            $"Required folder metadata is missing for project {projectId}, role {role}.");
    }

    public string ResolveFolder(Project project, string destinationFolder)
    {
        if (project.Id is null || !Enum.TryParse(destinationFolder, true, out ProjectFolderRole folderRole))
        {
            return destinationFolder;
        }

        return GetRequiredFolderName((int)project.Id, folderRole);
    }

    public List<ProjectFolder> GetExistingProjectFolders(int projectId)
    {
        return Enum.GetValues<ProjectFolderRole>()
            .Select(role => GetFolder(projectId, role))
            .Where(folder => folder is not null && folder.Exists)
            .Select(folder => folder!)
            .ToList();
    }

    public string GetRequiredFolderPath(int projectId, ProjectFolderRole role)
    {
        return GetRequiredFolder(projectId, role).AbsolutePath;
    }

    public string GetRequiredFolderName(int projectId, ProjectFolderRole role)
    {
        return GetRequiredFolder(projectId, role).FolderName;
    }

    public void CreateRequiredFolders(Project project)
    {
        if (project.Id is null)
        {
            _logger.LogWarning("Could not create required folders because project id was null");
            throw new ArgumentNullException(nameof(project.Id));
        }

        _logger.LogInformation("Creating required folders for project: {ProjectId}", project.Id);

        _files.FolderCreate(project.Path);

        foreach (ProjectFolderRole role in Enum.GetValues<ProjectFolderRole>())
        {
            CreateRequiredFolder(project, role);
        }
    }

    private void CreateRequiredFolder(Project project, ProjectFolderRole role)
    {
        if (project.Id is null)
        {
            throw new ArgumentNullException(nameof(project.Id));
        }

        string folderName = GetDefaultFolderName(role);
        string folderPath = _files.Combine(project.Path, folderName);
        string metadataKey = ToMetadataKey(role);

        _files.FolderCreate(folderPath);

        if (_projectMetadataService.GetProjectMetadata(project.Id.Value, metadataKey) is null)
        {
            _projectMetadataService.AddMetadataToProject(
                project.Id.Value, metadataKey, folderName
            );
        }

        _logger.LogInformation(
            "Created required folder for project {ProjectId}, role {FolderRole}, folder {FolderName}",
            project.Id, role, folderName);
    }

    private string GetDefaultFolderName(ProjectFolderRole role)
    {
        string? folderName = _configuration
            .GetSection(FolderNamesConfigKey)
            .GetSection(role.ToString())
            .GetChildren()
            .Select(child => child.Value?.Trim())
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        return folderName ?? role.ToString();
    }


    private static string ToMetadataKey(ProjectFolderRole role)
    {
        return role switch
        {
            ProjectFolderRole.Originals => OriginalsFolderMetadataKey,
            ProjectFolderRole.Editing => EditingFolderMetadataKey,
            ProjectFolderRole.Finals => FinalsFolderMetadataKey,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
        };
    }
    
    
        public void UpdateProjectFolderMetadata(Project project)
    {
        if (project.Id is null)
        {
            _logger.LogWarning("Could not initialise project folder metadata because project id was null");
            throw new ArgumentNullException(nameof(project.Id));
        }
        

        Dictionary<string, string[]> folderNamesByRole = GetConfiguredFolderNames();

        if (folderNamesByRole.Count == 0)
        {
            _logger.LogDebug("No configured folder names found for project folder mapping");
            return;
        }

        string[] folderNames = _files.GetDirectories(project.Path)
            .Select(directory => _files.GetPathEnd(directory))
            .ToArray();

        foreach ((string folderRole, string[] possibleFolderNames) in folderNamesByRole)
        {
            string[] matchingFolderNames = CompareFolderNames(folderNames, possibleFolderNames);
            string metadataKey = ToFolderMetadataKey(folderRole);

            if (matchingFolderNames.Length == 0)
            {
                _logger.LogDebug("No folder found for role {FolderRole} in project {ProjectId}", folderRole, project.Id);
                continue;
            }

            if (matchingFolderNames.Length > 1)
            {
                _logger.LogWarning("Multiple folders found for role {FolderRole} in project {ProjectId}: {FolderNames}", folderRole, project.Id, string.Join(", ", matchingFolderNames));
                continue;
            }

            _projectMetadataService.UpdateMetadataForProject(project.Id.Value, metadataKey, matchingFolderNames[0]);

            _logger.LogInformation("Mapped project folder role {FolderRole} to folder {FolderName} for project {ProjectId}", folderRole, matchingFolderNames[0], project.Id);
        }
    }
    
    private Dictionary<string, string[]> GetConfiguredFolderNames()
    {
        return _configuration
            .GetSection(FolderNamesConfigKey)
            .GetChildren()
            .ToDictionary(
                section => section.Key,
                section => section.GetChildren()
                    .Select(child => child.Value)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value!)
                    .ToArray());
    }
    
    public static string[] CompareFolderNames(string[] folderNames, string[] possibleFolderNames)
    {
        return folderNames
            .Where(folderName => possibleFolderNames.Contains(folderName, StringComparer.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static string ToFolderMetadataKey(string folderRole)
    {
        return FolderMetadataKeyPrefix + folderRole.ToLowerInvariant();
    }
}