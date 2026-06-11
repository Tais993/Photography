using Application.interfaces;
using Domain.entities;
using Microsoft.Extensions.Logging;
using static Application.Constants;

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

    public IEnumerable<Image> HideRawFilesWhenNonRawExists(IEnumerable<Image> images)
    {
        List<Image> imageList = images.ToList();

        // This gets a Set of all images that are NOT raws
        HashSet<string> nonRawFileNames = imageList
            .Where(image => !IsRaw(image.FileType))
            .Select(image => Path.GetFileNameWithoutExtension(image.FileName))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // afbeelding is RAW en heeft GEEN raw variant, dan is ie goedgekeurd
        // 
        return imageList.Where(image =>
            !IsRaw(image.FileType) ||
            !nonRawFileNames.Contains(Path.GetFileNameWithoutExtension(image.FileName))
        );
    }

    private static bool IsRaw(string fileType)
    {
        return RawFileTypes.Contains(fileType);
    }
}