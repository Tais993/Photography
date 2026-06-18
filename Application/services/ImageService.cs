using Application.interfaces;
using Application.services.interfaces;
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

    public Image GetImageById(int imageId)
    {
        return _imageRepository.GetById(imageId);
    }

    public List<Image> GetImagesByProjectId(int projectId)
    {
        return _imageRepository.GetAllByProjectId(projectId);
    }
    
    public int GetProjectImageCount(int projectId)
    {
        return _imageRepository.GetProjectImageCount(projectId);
    }
}