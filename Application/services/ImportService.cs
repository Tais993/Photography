using System.Collections.Concurrent;
using Application.interfaces.infrastructure;
using Application.interfaces.infrastructure.repositories;
using Application.interfaces.services;
using Application.interfaces.services.project;
using Domain.entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.services;

public class ProjectImportService : IProjectImportService
{
    private static readonly ConcurrentDictionary<Guid, ProjectImportProgress> ProgressByImportId = new();

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ProjectImportService> _logger;

    public ProjectImportService(IServiceScopeFactory serviceScopeFactory, ILogger<ProjectImportService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public Guid StartImport(ProjectImportRequest request)
    {
        Guid importId = Guid.NewGuid();

        ProgressByImportId[importId] = new ProjectImportProgress()
        {
            ImportId = importId,
            ProjectId = request.ProjectId,
            TotalFiles = request.FilePaths.Count,
            FilesImported = 0
        };

        _ = Task.Run(() => Import(importId, request));

        return importId;
    }

    public ProjectImportProgress? GetProgress(Guid importId)
    {
        ProgressByImportId.TryGetValue(importId, out ProjectImportProgress? progress);

        return progress;
    }

    private void Import(Guid importId, ProjectImportRequest request)
    {
        try
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            IProjectFolderService projectFolderService =
                scope.ServiceProvider.GetRequiredService<IProjectFolderService>();
            IProjectService projectService =
                scope.ServiceProvider.GetRequiredService<IProjectService>();
            IImageRepository imageRepository =
                scope.ServiceProvider.GetRequiredService<IImageRepository>();
            IThumbnailService thumbnailService =
                scope.ServiceProvider.GetRequiredService<IThumbnailService>();
            IFiles files =
                scope.ServiceProvider.GetRequiredService<IFiles>();

            
            string projectPath = projectService.GetProjectById(request.ProjectId)?.Path ??
                                 throw new Exception("Project not found");

            string originalsFolderPath = projectFolderService.GetRequiredFolderPath(
                request.ProjectId,
                ProjectFolderRole.Originals);

            _logger.LogInformation("Importing {Count} images into project {ProjectId}", request.FilePaths.Count,
                request.ProjectId);

            for (int i = 0; i < request.FilePaths.Count; i++)
            {
                string sourceFile = request.FilePaths[i];
                string targetFile = files.Combine(originalsFolderPath, files.GetFileName(sourceFile));

                _logger.LogDebug("Importing image {SourceFile} to {TargetFile}", sourceFile, targetFile);

                
                files.CopyFile(sourceFile, targetFile);

                Image image = CreateImage(request.ProjectId, projectPath, targetFile, files);
                Image insertedImage = imageRepository.Insert(image);

                if (request.RemoveSourceFilesAfterImport)
                {
                    files.DeleteFile(sourceFile);
                }

                
                UpdateProgress(importId, i + 1, files.GetFileName(sourceFile));
            }

            CompleteProgress(importId);
            
            _logger.LogInformation("Finished importing {Count} images into project {ProjectId}", request.FilePaths.Count, request.ProjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Import failed for project {ProjectId}", request.ProjectId);
            FailProgress(importId, ex.Message);
        }
    }
    
    
    
    private static void UpdateProgress(Guid importId, int filesImported, string currentFile)
    {
        if (!ProgressByImportId.TryGetValue(importId, out ProjectImportProgress? progress))
        {
            return;
        }

        progress.FilesImported = filesImported;
        progress.CurrentFile = currentFile;
    }
    
    private static void CompleteProgress(Guid importId)
    {
        if (!ProgressByImportId.TryGetValue(importId, out ProjectImportProgress? progress))
        {
            return;
        }

        progress.FilesImported = progress.TotalFiles;
        progress.CurrentFile = null;
        progress.IsCompleted = true;
    }
    
    private static void FailProgress(Guid importId, string errorMessage)
    {
        if (!ProgressByImportId.TryGetValue(importId, out ProjectImportProgress? progress))
        {
            return;
        }

        progress.HasFailed = true;
        progress.ErrorMessage = errorMessage;
        progress.CurrentFile = null;
    }
    

    private static Image CreateImage(int projectId, string projectPath, string targetFile, IFiles files)
    {
        return new Image(
            projectId: projectId,
            fileName: files.GetFileNameWithoutExtension(targetFile),
            fileType: files.GetFileExtension(targetFile),
            relationalFilePath: files.GetRelativePath(projectPath, targetFile
            ));
    }
}