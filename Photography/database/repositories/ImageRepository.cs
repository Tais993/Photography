using Npgsql;
using PhotographyNET.database.entities;

namespace PhotographyNET.database.repositories;

public class ImageRepository : AbstractRepository<Image>
{
    public ImageRepository(NpgsqlDataSource dataSource) : base(dataSource)
    {
    }

    public override Image? GetById(int id)
    {
        return QuerySingle("""
                          SELECT id, project_id, file_name, file_type, file_path FROM public.image 
                          WHERE id = ($1)
                          """, MapImage, id);
    }

    public override List<Image> GetAll()
    {
        return QueryMultiple("""
                           SELECT id, project_id, file_name, file_type, file_path FROM public.image 
                           """, MapImage);
    }
    
    public List<Image> GetAllByProjectId(int projectId)
    {
        return QueryMultiple("""
                             SELECT id, project_id, file_name, file_type, file_path FROM public.image
                             WHERE project_id = ($1)
                             """, MapImage, projectId);
    }

    public List<Image> GetAllByProject(Project project)
    {
        if (project?.Id is null) throw new Exception("yeah i need the actual project id");
        
        var images = QueryMultiple("""
                                   SELECT id, project_id, file_name, file_type, file_path FROM public.image
                                   WHERE project_id = ($1)
                                   """, MapImage, project.Id);

        foreach (var image in images)
        {
            image.project = project; 
        }

        return images;
    }

    public override Image Insert(Image image)
    {
        if (image?.Id is null) throw new Exception("yeah i need the actual image id");

        return QuerySingle("""
                           INSERT INTO public.image(project_id, file_name, file_type, file_path) 
                           VALUES ($1, $2, $3, $4)
                           RETURNING *
                           """, MapImage, image.ProjectId, image.FileName, image.FileType, image.FilePath) ?? throw new Exception("Insert failed");
    }

    public override void Update(Image image)
    {
        Execute("""
                UPDATE public.image
                SET project_id = $1,
                    file_name = $2,
                    file_type = $3,
                    file_path = $4
                WHERE id = $5
                """, image.ProjectId, image.FileName, image.FileType, image.FilePath, image.Id);
    }
    
    
    public override Image? DeleteById(int id)
    {
        return QuerySingle("""
                          DELETE FROM public.image 
                          WHERE id = ($1)
                          RETURNING id, project_id, file_name, file_type, file_path
                          """, MapImage, id);
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