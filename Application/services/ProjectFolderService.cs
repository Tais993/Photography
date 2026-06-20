using Application.interfaces.infrastructure;
using Application.interfaces.services;
using Domain.entities;
using Microsoft.Extensions.Logging;
using static Application.Constants;

namespace Application.services;

public class ProjectFolderService : IProjectFolderService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMetadataService _projectMetadataService;
    private readonly IFiles _files;
    private readonly ILogger<ProjectFolderService> _logger;

    public ProjectFolderService(
        IProjectRepository projectRepository,
        IProjectMetadataService projectMetadataService,
        IFiles files,
        ILogger<ProjectFolderService> logger)
    {
        _projectRepository = projectRepository;
        _projectMetadataService = projectMetadataService;
        _files = files;
        _logger = logger;
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
        if (project.Id is null 
            || !Enum.TryParse(destinationFolder, true, out ProjectFolderRole folderRole))
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
}