using System.Text;
using Application.interfaces.infrastructure;
using Domain.entities;
using Domain.entities.search;
using Microsoft.Extensions.Logging;
using static Application.Constants;
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
                         SELECT id, project_id, file_name, file_type, relational_file_path FROM public.image 
                         WHERE id = ($1)
                         """, MapImage, id);
    }
    
    public List<Image> GetAll()
    {
        _logger.LogDebug("Getting all images");

        List<Image> images = _db.QueryMultiple("""
                                               SELECT id, project_id, file_name, file_type, relational_file_path 
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
                                               SELECT id, project_id, file_name, file_type, relational_file_path
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
                                               SELECT id, project_id, file_name, file_type, relational_file_path FROM public.image
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
            image.ProjectId,
            image.FileName,
            image.FileType);

        Image insertedImage = _db.Query("""
                                        INSERT INTO public.image(project_id, file_name, file_type, relational_file_path) 
                                        VALUES ($1, $2, $3, $4)
                                        RETURNING *
                                        """, MapImage, image.ProjectId, image.FileName, image.FileType, image.RelationalFilePath);

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
                        relational_file_path = $4
                    WHERE id = $5
                    """, image.ProjectId, image.FileName, image.FileType, image.RelationalFilePath, image.Id);
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

        int count = _db.Query("""
                              SELECT COUNT(*)
                              FROM public.image
                              WHERE project_id = $1
                              """, _db.MapToInt,
            projectId);

        _logger.LogDebug("Project {ProjectId} has {Count} images", projectId, count);

        return count;
    }

    public List<Image> SearchImages(ImageSearchSettings imageSearchSettings)
    {
        _logger.LogDebug("Searching images");

        StringBuilder sqlBuilder = new();
        List<string> whereClauses = [];
        List<object?> parameters = [];

        sqlBuilder.Append("""
                          SELECT id, project_id, file_name, file_type, relational_file_path
                          FROM public.image
                          """);
        
        if (imageSearchSettings.ProjectId is not null)
        {
            parameters.Add(imageSearchSettings.ProjectId);
            whereClauses.Add($"project_id = ${parameters.Count}::int");
        }

        if (!string.IsNullOrWhiteSpace(imageSearchSettings.FileNumber))
        {
            parameters.Add(imageSearchSettings.FileNumber);
            whereClauses.Add($"file_name ~* ('(^|[^0-9])' || ${parameters.Count}::text || '([^0-9]|$)')");
        }
        
        if (!string.IsNullOrWhiteSpace(imageSearchSettings.FileName))
        {
            parameters.Add(imageSearchSettings.FileName);
            whereClauses.Add($"file_name ILIKE ('%' || ${parameters.Count}::text || '%')");
        }
        
        if (!string.IsNullOrWhiteSpace(imageSearchSettings.FolderName))
        {
            parameters.Add(imageSearchSettings.FolderName);
            whereClauses.Add($"replace(relational_file_path, chr(92), '/') LIKE (${parameters.Count}::text || '/%')");
        }
        
        if (!string.IsNullOrWhiteSpace(imageSearchSettings.FileType))
        {
            parameters.Add(imageSearchSettings.FileType);
            whereClauses.Add($"LOWER(file_type) = LOWER(${parameters.Count}::text)");
        }
        
        if (whereClauses.Count > 0)
        {
            sqlBuilder.AppendLine("\nWHERE " + string.Join("\n  AND ", whereClauses));
        }

        sqlBuilder.AppendLine("\nORDER BY id DESC, file_name");

        List<Image> images = _db.QueryMultiple(sqlBuilder.ToString(),
            MapImage, parameters.ToArray());

        _logger.LogDebug("Image search returned {Count} images", images.Count);

        return images;
    }


    private static Image MapImage(NpgsqlDataReader reader)
    {
        return new Image(
            (int)reader["id"],
            (int)reader["project_id"],
            null,
            (string)reader["file_name"],
            (string)reader["file_type"],
            (string)reader["relational_file_path"]
        );
    }
}