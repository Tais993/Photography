using Domain.entities;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure.database.repositories;

public class ImageRepository : IImageRepository
{
    private readonly RepositoryHelper _db;
    private readonly ILogger<ImageRepository> _logger;

    public ImageRepository(NpgsqlDataSource dataSource,
        ILogger<ImageRepository> logger,
        RepositoryHelper db)
    {
        _logger = logger;
        _db = db;
    }

    public Image GetById(int id)
    {
        return _db.Query("""
                         SELECT id, project_id, file_name, file_type, relational_file_path FROM public.image 
                         WHERE id = ($1)
                         """, MapImage, id);
    }
    
    public List<Image> GetAll()
    {
        return _db.QueryMultiple("""
                                 SELECT id, project_id, file_name, file_type, relational_file_path 
                                 FROM public.image 
                                 """, MapImage);
    }

    public List<Image> GetAllByIds(int[] imageIds)
    {
        if (imageIds.Length == 0) return [];
        
        return _db.QueryMultiple("""
                                 SELECT id, project_id, file_name, file_type, relational_file_path
                                 FROM public.image
                                 WHERE id = any($1)
                                 """, MapImage, imageIds);
        // sql
    }

    public List<Image> GetAllByProjectId(int projectId)
    {
        return _db.QueryMultiple("""
                                 SELECT id, project_id, file_name, file_type, relational_file_path FROM public.image
                                 WHERE project_id = ($1)
                                 """, MapImage, projectId);
    }

    public List<Image> GetAllByProject(Project project)
    {
        if (project.Id is null) throw new ArgumentException("Project must have an ID", nameof(project));

        return GetAllByProjectId((int)project.Id);
    }

    public Image Insert(Image image)
    {
        return _db.Query("""
                         INSERT INTO public.image(project_id, file_name, file_type, relational_file_path) 
                         VALUES ($1, $2, $3, $4)
                         RETURNING *
                         """, MapImage, image.ProjectId, image.FileName, image.FileType, image.RelationalFilePath);
    }

    public void Update(Image image)
    {
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
        _db.Execute("""
                    DELETE FROM public.image 
                    WHERE id = ($1)
                    """, id);
    }

    public int GetProjectImageCount(int projectId)
    {
        return _db.Query("""
                         SELECT COUNT(*)
                         FROM public.image
                         WHERE project_id = $1
                         """, reader => !reader.HasRows ? 0 : reader.GetInt32(0),
            projectId);
    }

    public List<Image> SearchImages(ImageSearchSettings imageSearchSettings)
    {
        return _db.QueryMultiple("""
                                 SELECT id, project_id, file_name, file_type, relational_file_path
                                 FROM public.image
                                 WHERE ($1::int IS NULL OR project_id = $1::int)
                                   AND (
                                     $2::text IS NULL
                                         OR file_name ~* ('(^|[^0-9])' || $2::text || '([^0-9]|$)')
                                     )
                                   AND (
                                     $3::text IS NULL
                                         OR file_name ILIKE ('%' || $3::text || '%')
                                     )
                                   AND (
                                     $4::text IS NULL
                                         OR replace(relational_file_path, chr(92), '/') LIKE ($4::text || '/%')
                                     )
                                   AND (
                                     $5::text IS NULL
                                         OR LOWER(file_type) = LOWER($5::text)
                                     )
                                 """, MapImage, imageSearchSettings.ProjectId!, imageSearchSettings.FileNumber!,
            imageSearchSettings.FileName!, imageSearchSettings.FolderName!, imageSearchSettings.FileType!);
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