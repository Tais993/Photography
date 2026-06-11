using Application.interfaces;
using Application.services.interfaces;
using Domain.entities;
using ImageMagick;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Image = Domain.entities.Image;

namespace Application.services;

public class ThumbnailService : IThumbnailService
{
    private readonly IProjectService _projectService;
    private readonly IImageService _imageService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ThumbnailService> _logger;
    private readonly IFiles _files;

    private readonly int _defaultSize;
    private readonly int _largeSize;
    private readonly int _jpegQuality;

    public ThumbnailService(
        IProjectService projectService, IFiles files, IConfiguration configuration,
        ILogger<ThumbnailService> logger, IImageService imageService)
    {
        _projectService = projectService;
        _files = files;
        _configuration = configuration;
        _logger = logger;
        _imageService = imageService;

        _defaultSize = _configuration.GetValue<int>("Thumbnails:DefaultSize", 300);
        _largeSize = _configuration.GetValue<int>("Thumbnails:LargeSize", 1200);
        _jpegQuality = _configuration.GetValue<int>("Thumbnails:JpegQuality", 80);
    }

    public ThumbnailResult GetThumbnail(int imageId, string size = "default")
    {
        Image? image = _imageService.GetImageById(imageId);

        if (image is null)
        {
            return ThumbnailResult.NotFound();
        }

        Project? project = _projectService.GetProjectById(image.ProjectId);

        if (project is null)
        {
            return ThumbnailResult.NotFound();
        }

        string fullPath = _files.Combine(project.Path, image.RelationalFilePath);

        if (!File.Exists(fullPath))
        {
            _logger.LogWarning("Original image file was not found: {Path}", fullPath);
            return ThumbnailResult.NotFound();
        }

        int maxSize = GetMaxSize(size);
        string cachePath = GetThumbnailCachePath(imageId, size, maxSize);

        if (!ThumbnailCacheIsValid(fullPath, cachePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);
            GenerateThumbnail(fullPath, cachePath, maxSize);
        }

        return ThumbnailResult.Success(cachePath);
    }

    private int GetMaxSize(string size)
    {
        return size.Equals("large", StringComparison.OrdinalIgnoreCase)
            ? _largeSize
            : _defaultSize;
    }

    /// <summary>
    /// Based on the original file's path, it writes a thumbnail version of the file to the given cachePath
    /// </summary>
    /// <param name="originalPath"></param>
    /// <param name="cachePath"></param>
    /// <param name="maxSize"></param>
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

    /// <summary>
    /// Based on the image ID and size, it figures out at what location the thumbnail should be found.
    /// </summary>
    /// <param name="imageId"></param>
    /// <param name="size"></param>
    /// <param name="maxSize"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Validates if the current cached thumbnail is up to date.
    /// </summary>
    /// <param name="originalPath">The file path of the original image.</param>
    /// <param name="cachePath">The file path of the cached thumbnail.</param>
    /// <returns>
    /// </returns>
    private static bool ThumbnailCacheIsValid(string originalPath, string cachePath)
    {
        if (!File.Exists(cachePath))
        {
            return false;
        }

        DateTime originalModified = File.GetLastWriteTimeUtc(originalPath);
        DateTime cacheModified = File.GetLastWriteTimeUtc(cachePath);

        return cacheModified >= originalModified;
    }
}