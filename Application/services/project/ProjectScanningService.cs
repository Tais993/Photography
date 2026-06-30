using Application.interfaces.infrastructure;
using Application.interfaces.services.project;
using Domain.entities;
using Microsoft.Extensions.Logging;
using static Application.Constants;

namespace Application.services.project;

public class ProjectScanningService : IProjectScanningService
{
    private readonly IProjectStorageService _projectStorage;
    private readonly IImageRepository _imageRepository;
    private readonly IImageMetadataRepository _imageMetadataRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IFiles _files;
    private readonly ILogger<ProjectScanningService> _logger;

    public ProjectScanningService(IImageRepository imageRepository, IProjectRepository projectRepository, IFiles files, ILogger<ProjectScanningService> logger, IImageMetadataRepository imageMetadataRepository, IProjectStorageService projectStorage)
    {
        _imageRepository = imageRepository;
        _projectRepository = projectRepository;
        _files = files;
        _logger = logger;
        _imageMetadataRepository = imageMetadataRepository;
        _projectStorage = projectStorage;
    }

    public void ScanProject(Project project)
    {
        if (project.Id is null)
        {
            _logger.LogWarning("Could not scan project because project id was null");
            throw new ArgumentNullException(nameof(project.Id));
        }

        _logger.LogInformation("Scanning project: {ProjectId}", project.Id);

        DeleteMissingImages(project);
        ScanProjectFolders(project);
        ScanSubProjects(project);
        _projectStorage.UpdateStorageInfo(project);
        
        _logger.LogInformation("Finished scanning project: {ProjectId}", project.Id);
    }

    private void ScanProjectFolders(Project project)
    {
        string[] subDirectories = _files.GetDirectories(project.Path);
        _logger.LogDebug("Scanning {Count} project subdirectories for project: {ProjectId}", subDirectories.Length, project.Id);

        HashSet<string> existingImagePaths = _imageRepository.GetAllByProjectId(project.Id!.Value)
            .Select(image => image.RelationalFilePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (string subDirectory in subDirectories)
        {
            string folderName = _files.GetPathEnd(subDirectory);

            if (IsSubProjectFolder(folderName))
            {
                _logger.LogDebug("Skipping subproject folder while scanning project images: {FolderName}", folderName);
                continue;
            }

            ScanProjectFolder(project, subDirectory, existingImagePaths);
        }

        _logger.LogInformation("Finished scanning project folders for project: {ProjectId}", project.Id);
    }

    private void ScanProjectFolder(Project project, string projectFolder, HashSet<string> existingImagePaths)
    {
        string[] files = _files.GetFiles(projectFolder);

        _logger.LogDebug("Scanning {Count} files for project: {ProjectId}, folder: {FolderName}", files.Length, project.Id, projectFolder);

        foreach (string filePath in files)
        {
            if (!IsImageFile(filePath))
            {
                _logger.LogTrace("Skipping non-image file: {FilePath}", filePath);
                continue;
            }

            string relativeFilePath = _files.GetRelativePath(project.Path, filePath);

            if (existingImagePaths.Contains(relativeFilePath))
            {
                _logger.LogTrace("Image already exists in database: {RelativeFilePath}", relativeFilePath);
                continue;
            }

            InsertImage(project, filePath);
            existingImagePaths.Add(relativeFilePath);
        }

        _logger.LogInformation("Finished scanning folder for project: {ProjectId}", project.Id);
    }

    private void ScanSubProjects(Project project)
    {
        List<Project> subProjects = _projectRepository.GetAllByParentProjectId(project.Id!.Value);
        _logger.LogDebug("Scanning {Count} subprojects for project: {ProjectId}", subProjects.Count, project.Id);

        foreach (Project subProject in subProjects)
        {
            ScanProject(subProject);
        }
    }

    private void DeleteMissingImages(Project project)
    {
        List<Image> images = _imageRepository.GetAllByProjectId(project.Id!.Value);

        _logger.LogDebug("Checking {Count} existing images for project: {ProjectId}", images.Count, project.Id);

        foreach (Image image in images)
        {
            string fullPath = _files.Combine(project.Path, image.RelationalFilePath);

            if (_files.Exists(fullPath))
            {
                continue;
            }

            image.ImageStatus = ImageStatus.Unavailable;
            _imageRepository.Update(image);

            _logger.LogInformation("Deleted image because it was not found on the filesystem: {RelativeFilePath}", image.RelationalFilePath);
        }

        _logger.LogInformation("Finished checking missing images for project: {ProjectId}", project.Id);
    }

    private void InsertImage(Project project, string filePath)
    {
        string fileExtension = _files.GetFileExtension(filePath);
        string fileName = _files.GetFileNameWithoutExtension(filePath);
        string relativeFilePath = _files.GetRelativePath(project.Path, filePath);

        Image image = new Image(project.Id!.Value, fileName, fileExtension, relativeFilePath);
        _imageRepository.Insert(image);

        _logger.LogInformation("Inserted new image file: {RelativeFilePath}", relativeFilePath);
    }

    private bool IsImageFile(string filePath)
    {
        string fileExtension = _files.GetFileExtension(filePath);

        return ImageFileTypes.Contains(fileExtension)
               || RawFileTypes.Contains(fileExtension);
    }

    private static bool IsSubProjectFolder(string folderName)
    {
        return SubProjectNameRegex.Match(folderName).Success;
    }
}