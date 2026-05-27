using Application.services.interfaces;
using Infrastructure.database.repositories;
using Infrastructure.filesystem;
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
        return _imageRepository.GetAllByIds(imageIds)
            .Select(image => image.RelationalFilePath);
    }


    public void CopyFiles(IEnumerable<string> relativeFiles, string projectPath, string relativeTargetDirectory)
    {
        string targetDirectory = _files.Combine(projectPath, relativeTargetDirectory);

        foreach (string relativeFile in relativeFiles)
        {
            string originalFile = _files.Combine(projectPath, relativeFile);
            string targetFile = _files.Combine(targetDirectory, _files.GetFileName(relativeFile));

            _logger.LogDebug($"Original file {originalFile}, Target file {targetFile}");
            
            _files.CopyFile(originalFile, targetFile);
        }
    }
}