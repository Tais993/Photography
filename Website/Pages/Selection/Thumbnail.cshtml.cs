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

    public ThumbnailModel(ISearchService searchService, IProjectService projectService, ILogger<ThumbnailModel> logger,
        IFiles files, IConfiguration configuration)
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
        Image image = _projectService.GetImageById(imageId);
        Project? project = _projectService.GetProjectById(image.ProjectId);

        if (image is null || project is null)
        {
            return NotFound();
        }

        string fullPath = _files.Combine(project.Path, image.RelationalFilePath);

        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound();
        }

        int maxSize = size.Equals("large", StringComparison.OrdinalIgnoreCase)
            ? _largeSize
            : _defaultSize;

        string cachePath = GetThumbnailCachePath(imageId, size, maxSize);

        if (ThumbnailCacheIsValid(fullPath, cachePath))
        {
            SetThumbnailCacheHeaders();
            return PhysicalFile(cachePath, "image/jpeg");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);

        GenerateThumbnail(fullPath, cachePath, maxSize);

        SetThumbnailCacheHeaders();
        return PhysicalFile(cachePath, "image/jpeg");
    }

    private void GenerateThumbnail(string originalPath, string cachePath, int maxSize)
    {
        uint thumbnailSize = (uint)maxSize;

        using MagickImage imageFile = new(originalPath);

        imageFile.AutoOrient();

        imageFile.Resize(new MagickGeometry(thumbnailSize, thumbnailSize)
        {
            IgnoreAspectRatio = false
        });

        imageFile.Format = MagickFormat.Jpeg;
        imageFile.Quality = (uint)_jpegQuality;

        imageFile.Write(cachePath);
    }

    private string GetThumbnailCachePath(int imageId, string size, int maxSize)
    {
        string cacheRoot = _configuration.GetValue<string>("Thumbnails:CachePath") ??
                           Path.Combine(AppContext.BaseDirectory, "thumbnail-cache");

        string safeSize = size.Equals("large", StringComparison.OrdinalIgnoreCase)
            ? "large"
            : "default";

        return Path.Combine(
            cacheRoot,
            safeSize,
            $"{imageId}_{maxSize}_q{_jpegQuality}.jpg"
        );
    }

    private static bool ThumbnailCacheIsValid(string originalPath, string cachePath)
    {
        if (!System.IO.File.Exists(cachePath))
        {
            return false;
        }

        DateTime originalModified = System.IO.File.GetLastWriteTimeUtc(originalPath);
        DateTime cacheModified = System.IO.File.GetLastWriteTimeUtc(cachePath);

        return cacheModified >= originalModified;
    }
    
    private void SetThumbnailCacheHeaders()
    {
        Response.Headers.CacheControl = "public,max-age=604800";
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