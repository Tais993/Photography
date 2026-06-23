using Application.interfaces.infrastructure;
using Application.interfaces.services.project;
using Domain.entities;
using Microsoft.Extensions.Logging;

namespace Application.services.project;

public class ProjectScanningService : IProjectScanningService
{
    private readonly IImageRepository _imageRepository;
    private readonly IFiles _files;
    private readonly ILogger<ProjectScanningService> _logger;

    public ProjectScanningService(
        IImageRepository imageRepository,
        IFiles files,
        ILogger<ProjectScanningService> logger)
    {
        _imageRepository = imageRepository;
        _files = files;
        _logger = logger;
    }
    
    public void ScanProject(Project project)
    {
        if (project.Id is null)
        {
            _logger.LogWarning("Could not scan project because project id was null");
            throw new ArgumentNullException(nameof(project.Id));
        }

        ScanProjectSubFolders(project.Path, project.Id.Value);
    }

    
    public void ScanProjectSubFolders(string projectDirectory, int projectId)
    {
        if (projectId == null || projectId == 0)
        {
            _logger.LogWarning("Could not scan project subfolders because project id was null");
            throw new ArgumentNullException(nameof(projectId));
        }

        string[] subDirectories = _files.GetDirectories(projectDirectory);

        _logger.LogDebug("Scanning {Count} project subdirectories for project: {ProjectId}", subDirectories.Length, projectId);

        foreach (string subDirectory in subDirectories)
        {
            ScanProjectSubFolder(projectDirectory, subDirectory, projectId);
        }
    }


    public void ScanProjectSubFolder(string projectDirectory, string projectSubDirectory, int projectId)
    {
        string[] files = _files.GetFiles(projectSubDirectory);

        _logger.LogDebug("Scanning {Count} images for project: {ProjectId}, folder: {FolderName}", files.Length, projectId, projectSubDirectory);

        foreach (string filePath in files)
        {
            InsertImage(projectDirectory, filePath, projectId);
        }

        _logger.LogDebug(
            "Finished scanning {Count} images for project: {ProjectId}, folder: {FolderName}", files.Length, projectId, projectSubDirectory);
    }

    private void InsertImage(string projectDirectory, string filePath, int projectId)
    {
        string fileExtension = _files.GetFileExtension(filePath);
        string fileName = _files.GetFileName(filePath).Replace(fileExtension, "");
        string relativeFilePath = _files.GetRelativePath(projectDirectory, filePath);

        Image image = new Image(projectId, fileName, fileExtension, relativeFilePath);

        _imageRepository.Insert(image);

        _logger.LogTrace("Inserted image file: {RelativeFilePath}", relativeFilePath);
    }
}