using Application.services.interfaces;
using Domain.entities;
using Infrastructure.filesystem;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Website.Pages.Selection;

public class ThumbnailModel : PageModel
{
    private readonly ISearchService _searchService;
    private readonly IProjectService _projectService;
    private readonly ILogger<ThumbnailModel> _logger;
    private readonly IFiles _files;

    public ThumbnailModel(ISearchService searchService, IProjectService projectService, ILogger<ThumbnailModel> logger, IFiles files)
    {
        _searchService = searchService;
        _projectService = projectService;
        _logger = logger;
        _files = files;
    }

    public IActionResult OnGet(int imageId)
    {
        _logger.LogInformation($"ImageId: {imageId}");
        
        Image image = _projectService.GetImageById(imageId);

        _logger.LogInformation($"Image: {image}");
        
        if (image is null)
        {
            return NotFound();
        }

        _logger.LogInformation($"Image is found");
        
        Project? project = _projectService.GetProjectById(image.ProjectId);

        if (project is null)
        {
            return NotFound();
        }

        _logger.LogInformation($"Project is found");
        string fullPath = _files.Combine(project.Path, image.RelationalFilePath);
        _logger.LogInformation($"FullPath: {fullPath}");
        
        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound();
        }

        _logger.LogInformation($"File is found");
        

        string? contentType = GetContentType(image.FileType);

        _logger.LogInformation($"Thumbnail content type: {contentType}");
        _logger.LogInformation($"Thumbnail path: {fullPath}");
        
        return new PhysicalFileResult(fullPath, contentType);
    }

    private static string? GetContentType(string fileType)
    {
        return fileType.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => null
        };
    }
}