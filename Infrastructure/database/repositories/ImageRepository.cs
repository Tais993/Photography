using Application.interfaces.infrastructure;
using Application.interfaces.infrastructure.repositories;
using Domain.entities;
using Microsoft.Extensions.Logging;
using static Infrastructure.database.repositories.DatabaseMappers;

namespace Infrastructure.database.repositories;

public class ImageRepository : IImageRepository
{
    private readonly RepositoryHelper _db;
    private readonly ILogger<ImageRepository> _logger;

    public ImageRepository(
        ILogger<ImageRepository> logger,
        RepositoryHelper db)
    {
        _logger = logger;
        _db = db;
    }

    public Image? GetById(int id)
    {
        _logger.LogDebug("Getting image by id: {ImageId}", id);

        return _db.QueryOrDefault("""
                                  SELECT id, project_id, file_name, file_type, relational_file_path, status 
                                  FROM public.image 
                                  WHERE id = ($1)
                                  """, MapImage, id);
    }

    public List<Image> GetAll()
    {
        _logger.LogDebug("Getting all images");

        List<Image> images = _db.QueryMultiple("""
                                               SELECT id, project_id, file_name, file_type, relational_file_path, status 
                                               FROM public.image 
                                               """, MapImage);

        _logger.LogDebug("Found {Count} images", images.Count);

        return images;
    }

    public List<Image> GetAllByIds(int[] imageIds)
    {
        _logger.LogDebug("Getting images by ids, count: {Count}", imageIds.Length);

        if (imageIds.Length == 0)
        {
            return [];
        }

        List<Image> images = _db.QueryMultiple("""
                                               SELECT id, project_id, file_name, file_type, relational_file_path, status
                                               FROM public.image
                                               WHERE id = any($1)
                                               """, MapImage, imageIds);

        _logger.LogDebug("Found {Count} images by ids", images.Count);

        return images;
    }

    public List<Image> GetAllByProjectId(int projectId)
    {
        _logger.LogDebug("Getting images for project: {ProjectId}", projectId);

        List<Image> images = _db.QueryMultiple("""
                                               SELECT id, project_id, file_name, file_type, relational_file_path, status
                                               FROM public.image
                                               WHERE project_id = ($1)
                                               """, MapImage, projectId);

        _logger.LogDebug("Found {Count} images for project: {ProjectId}", images.Count, projectId);

        return images;
    }

    public List<Image> GetAllByProject(Project project)
    {
        if (project.Id is null)
        {
            _logger.LogWarning("Could not get images for project because project id was null");
            throw new ArgumentException("Project must have an ID", nameof(project));
        }

        return GetAllByProjectId((int)project.Id);
    }

    public Image Insert(Image image)
    {
        _logger.LogDebug(
            "Inserting image for project: {ProjectId}, file: {FileName}{FileType}",
            image.ProjectId, image.FileName, image.FileType);

        Image insertedImage = _db.Query("""
                                        INSERT INTO public.image(project_id, file_name, file_type, relational_file_path, status) 
                                        VALUES ($1, $2, $3, $4, $5)
                                        RETURNING *
                                        """, MapImage, image.ProjectId, image.FileName, image.FileType, image.RelationalFilePath, ImageStatusMapper.ToDatabaseValue(image.ImageStatus));

        _logger.LogTrace("Inserted image: {ImageId}", insertedImage.Id);

        return insertedImage;
    }

    public void Update(Image image)
    {
        _logger.LogDebug("Updating image: {ImageId}", image.Id);

        _db.Execute("""
                    UPDATE public.image
                    SET project_id = $1,
                        file_name = $2,
                        file_type = $3,
                        relational_file_path = $4,
                        status = $5
                    WHERE id = $6
                    """, image.ProjectId, image.FileName, image.FileType, image.RelationalFilePath, ImageStatusMapper.ToDatabaseValue(image.ImageStatus), image.Id);
    }


    public void DeleteById(int id)
    {
        _logger.LogDebug("Deleting image: {ImageId}", id);

        _db.Execute("""
                    DELETE FROM public.image 
                    WHERE id = ($1)
                    """, id);
    }

    public int GetProjectImageCount(int projectId)
    {
        _logger.LogDebug("Getting image count for project: {ProjectId}", projectId);

        int count = _db.QueryScalar<int>("""
                              SELECT COUNT(*)
                              FROM public.image
                              WHERE project_id = $1
                              """, projectId);

        _logger.LogDebug("Project {ProjectId} has {Count} images", projectId, count);
        return count;
    }
}