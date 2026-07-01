using Application.interfaces.infrastructure;
using Application.interfaces.infrastructure.repositories;
using Application.interfaces.services;
using Domain.entities;
using Microsoft.Extensions.Logging;

namespace Application.services;

public class ImageService : IImageService
{
    private readonly IImageRepository _imageRepository;
    private readonly ILogger<ImageService> _logger;

    public ImageService(
        IImageRepository imageRepository,
        ILogger<ImageService> logger)
    {
        _imageRepository = imageRepository;
        _logger = logger;
    }

    public Image? GetImageById(int imageId)
    {
        _logger.LogDebug("Getting image by id: {ImageId}", imageId);
        return _imageRepository.GetById(imageId);
    }

    public List<Image> GetImagesByProjectId(int projectId)
    {
        _logger.LogDebug("Getting images for project: {ProjectId}", projectId);

        List<Image> images = _imageRepository.GetAllByProjectId(projectId);

        _logger.LogDebug("Found {Count} images for project: {ProjectId}", images.Count, projectId);

        return images;
    }

    public int GetProjectImageCount(int projectId)
    {
        _logger.LogDebug("Getting image count for project: {ProjectId}", projectId);

        int count = _imageRepository.GetProjectImageCount(projectId);

        _logger.LogDebug("Project {ProjectId} has {Count} images", projectId, count);

        return count;
    }
}