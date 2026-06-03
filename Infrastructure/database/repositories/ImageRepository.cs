using System.Text;
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
                         """, _db.MapToInt,
            projectId);
    }

    public List<Image> SearchImages(ImageSearchSettings imageSearchSettings)
    {
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
        
        return _db.QueryMultiple(sqlBuilder.ToString(),
            MapImage, parameters.ToArray());
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