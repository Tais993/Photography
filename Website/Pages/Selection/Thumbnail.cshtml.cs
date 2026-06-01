using Application.services.interfaces;
using Domain.entities;
using ImageMagick;
using Infrastructure.filesystem;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Image = Domain.entities.Image;

namespace Website.Pages.Selection;

public class ThumbnailModel : PageModel
{
    private readonly ISearchService _searchService;
    private readonly IProjectService _projectService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ThumbnailModel> _logger;
    private readonly IFiles _files;

    private int _defaultSize;
    private int _largeSize;
    private int _jpegQuality;

    public ThumbnailModel(ISearchService searchService, IProjectService projectService, ILogger<ThumbnailModel> logger, IFiles files, IConfiguration configuration)
    {
        _searchService = searchService;
        _projectService = projectService;
        _logger = logger;
        _files = files;
        _configuration = configuration;

        _defaultSize = _configuration.GetValue<int>("Thumbnails:DefaultSize", 300);
        _largeSize = _configuration.GetValue<int>("Thumbnails:LargeSize", 1200);
        _jpegQuality = _configuration.GetValue<int>("Thumbnails:JpegQuality", 80);
    }

    public IActionResult OnGet(int imageId, string size = "default")
    {
        _logger.LogInformation($"ImageId: {imageId}");
        
        Image image = _projectService.GetImageById(imageId);
        Project? project = _projectService.GetProjectById(image.ProjectId);

        if (image is null || project is null)
        {
            return NotFound();
        }

        _logger.LogInformation($"Image is found");
        _logger.LogInformation($"Project is found");
        
        string fullPath = _files.Combine(project.Path, image.RelationalFilePath);
        
        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound();
        }
        
        int maxSize = size.Equals("large", StringComparison.OrdinalIgnoreCase)
            ? _largeSize
            : _defaultSize;

        uint thumbnailSize = (uint) maxSize;
        
        using MagickImage imageFile = new(fullPath);

        imageFile.AutoOrient();

        imageFile.Resize(new MagickGeometry(thumbnailSize, thumbnailSize)
        {
            IgnoreAspectRatio = false
        });

        imageFile.Format = MagickFormat.Jpeg;
        imageFile.Quality = (uint)_jpegQuality;

        using MemoryStream stream = new();
        imageFile.Write(stream);

        return File(stream.ToArray(), "image/jpeg");
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