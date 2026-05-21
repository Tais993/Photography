using Application.services.interfaces;
using Infrastructure.filesystem;
using Microsoft.Extensions.Logging;

namespace Application.services;

public class CopyService : ICopyService
{
    private readonly ILogger<CopyService> _logger;
    private readonly IFiles _files;

    public CopyService(ILogger<CopyService> logger, IFiles files)
    {
        _logger = logger;
        _files = files;
    }


    public void CopyFiles(IEnumerable<string> relativeFiles, string projectLocation, string relativeTargetDirectory)
    {
        string targetDirectory = _files.Combine(projectLocation, relativeTargetDirectory);

        foreach (string relativeFile in relativeFiles)
        {
            string originalFile = _files.Combine(projectLocation, relativeFile);
            string targetFile = _files.Combine(targetDirectory, _files.GetFileName(relativeFile));

            _logger.LogDebug($"Original file {originalFile}, Target file {targetFile}");
            
            _files.CopyFile(originalFile, targetFile);
        }
    }
}