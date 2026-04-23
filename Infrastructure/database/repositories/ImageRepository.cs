using Domain.entities;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure.database.repositories;

public class ImageRepository
{
    private RepositoryHelper _db;
    private ILogger<ImageRepository> _logger;

    public ImageRepository(NpgsqlDataSource dataSource,
        ILogger<ImageRepository> logger,
        RepositoryHelper db)
    {
        this._logger = logger;
        this._db = db;
    }

    public Image GetByKey(int id)
    {
        return _db.Query("""
                               SELECT id, project_id, file_name, file_type, file_path FROM public.image 
                               WHERE id = ($1)
                               """, MapImage, id);
    }

    public List<Image> GetAll()
    {
        return _db.QueryMultiple("""
                                 SELECT id, project_id, file_name, file_type, file_path FROM public.image 
                                 """, MapImage);
    }

    public List<Image> GetAllByProjectId(int projectId)
    {
        return _db.QueryMultiple("""
                                 SELECT id, project_id, file_name, file_type, file_path FROM public.image
                                 WHERE project_id = ($1)
                                 """, MapImage, projectId);
    }

    public List<Image> GetAllByProject(Project project)
    {
        if (project?.Id is null) throw new ArgumentException("Project must have an ID", nameof(project));

        return GetAllByProjectId((int)project.Id);
    }

    public Image Insert(Image image)
    {
        if (image?.Id is null) throw new ArgumentException("Image must have an ID", nameof(image));

        return _db.Query("""
                         INSERT INTO public.image(project_id, file_name, file_type, file_path) 
                         VALUES ($1, $2, $3, $4)
                         RETURNING *
                         """, MapImage, image.ProjectId, image.FileName, image.FileType, image.FilePath);
    }

    public void Update(Image image)
    {
        _db.Execute("""
                UPDATE public.image
                SET project_id = $1,
                    file_name = $2,
                    file_type = $3,
                    file_path = $4
                WHERE id = $5
                """, image.ProjectId, image.FileName, image.FileType, image.FilePath, image.Id);
    }


    public void DeleteByKey(int id)
    {
        _db.Execute("""
                DELETE FROM public.image 
                WHERE id = ($1)
                """, id);
    }

    private static Image MapImage(NpgsqlDataReader reader)
    {
        return new Image(
            (int)reader["id"],
            (int)reader["project_id"],
            null,
            (string)reader["file_name"],
            (string)reader["file_type"],
            (string)reader["file_path"]
        );
    }
}