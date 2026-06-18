using Application.interfaces.infrastructure;
using Application.interfaces.services;
using Microsoft.Extensions.Logging;

namespace Application.services;

public class CopyService : ICopyService
{
    private readonly IImageRepository _imageRepository;
    private readonly ILogger<CopyService> _logger;
    private readonly IFiles _files;

    public CopyService(ILogger<CopyService> logger, IFiles files, IImageRepository imageRepository)
    {
        _logger = logger;
        _files = files;
        _imageRepository = imageRepository;
    }

    public IEnumerable<string> ImageIdsToRelativePaths(int[] imageIds)
    {
        _logger.LogDebug("Resolving relative paths for {Count} images", imageIds.Length);

        List<string> relativePaths = _imageRepository.GetAllByIds(imageIds)
            .Select(image => image.RelationalFilePath)
            .ToList();

        _logger.LogDebug("Resolved {Count} relative paths", relativePaths.Count);

        return relativePaths;
    }


    public void CopyFiles(IEnumerable<string> relativeFiles, string projectPath, string relativeTargetDirectory)
    {
        List<string> files = relativeFiles.ToList();
        string targetDirectory = _files.Combine(projectPath, relativeTargetDirectory);

        _logger.LogInformation("Copying {Count} files to directory: {TargetDirectory}", files.Count, targetDirectory);

        foreach (string relativeFile in files)
        {
            string originalFile = _files.Combine(projectPath, relativeFile);
            string targetFile = _files.Combine(targetDirectory, _files.GetFileName(relativeFile));

            _logger.LogDebug("Original file {OriginalFile}, Target file {TargetFile}", originalFile, targetFile);

            _files.CopyFile(originalFile, targetFile);
        }

        _logger.LogInformation("Finished copying {Count} files to directory: {TargetDirectory}", files.Count, targetDirectory);
    }
}