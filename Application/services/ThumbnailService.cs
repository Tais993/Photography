using Application.interfaces.infrastructure;
using Application.interfaces.services;
using Domain.entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Image = Domain.entities.Image;

namespace Application.services;

public class ThumbnailService : IThumbnailService
{
    private readonly IThumbnailGenerator _thumbnailGenerator;
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
        ILogger<ThumbnailService> logger, IImageService imageService, IThumbnailGenerator thumbnailGenerator)
    {
        _projectService = projectService;
        _files = files;
        _configuration = configuration;
        _logger = logger;
        _imageService = imageService;
        _thumbnailGenerator = thumbnailGenerator;

        _defaultSize = _configuration.GetValue<int>("Thumbnails:DefaultSize", 300);
        _largeSize = _configuration.GetValue<int>("Thumbnails:LargeSize", 1200);
        _jpegQuality = _configuration.GetValue<int>("Thumbnails:JpegQuality", 80);
    }

    public ThumbnailResult GetThumbnail(int imageId, string size = "default")
    {
        _logger.LogDebug("Getting thumbnail for image: {ImageId}, size: {Size}", imageId, size);

        Image? image = _imageService.GetImageById(imageId);

        if (image is null)
        {
            _logger.LogWarning("Thumbnail could not be created, image was not found: {ImageId}", imageId);
            return ThumbnailResult.NotFound();
        }

        Project? project = _projectService.GetProjectById(image.ProjectId);

        if (project is null)
        {
            _logger.LogWarning("Thumbnail could not be created, project was not found: {ProjectId}", image.ProjectId);
            return ThumbnailResult.NotFound();
        }

        string fullPath = _files.Combine(project.Path, image.RelationalFilePath);

        if (!_files.Exists(fullPath))
        {
            _logger.LogWarning("Original image file was not found: {Path}", fullPath);
            return ThumbnailResult.NotFound();
        }

        int maxSize = GetMaxSize(size);
        string cachePath = GetThumbnailCachePath(imageId, size, maxSize);

        if (!ThumbnailCacheIsValid(fullPath, cachePath))
        {
            _logger.LogInformation("Generating thumbnail for image: {ImageId}, cache path: {CachePath}", imageId, cachePath);
            _files.FolderCreate(_files.GetDirectoryName(cachePath)!);
            _thumbnailGenerator.GenerateThumbnail(fullPath, cachePath, maxSize, (uint) _jpegQuality);
        }
        else
        {
            _logger.LogDebug("Thumbnail cache is valid for image: {ImageId}, cache path: {CachePath}", imageId, cachePath);
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
    private bool ThumbnailCacheIsValid(string originalPath, string cachePath)
    {
        if (!_files.Exists(cachePath))
        {
            _logger.LogDebug("Thumbnail cache does not exist: {CachePath}", cachePath);
            return false;
        }

        DateTime originalModified = _files.GetLastWriteTimeUtc(originalPath);
        DateTime cacheModified = _files.GetLastWriteTimeUtc(cachePath);

        bool cacheIsValid = cacheModified >= originalModified;

        if (!cacheIsValid)
        {
            _logger.LogDebug("Thumbnail cache is stale. Original: {OriginalPath}, cache: {CachePath}", originalPath, cachePath);
        }

        return cacheIsValid;
    }
}