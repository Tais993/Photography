using Domain.entities;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure.database.repositories;

public class SelectionRepository
{
    private readonly RepositoryHelper _db;
    private readonly ILogger<SelectionRepository> _logger;

    public SelectionRepository(NpgsqlDataSource dataSource,
        ILogger<SelectionRepository> logger,
        RepositoryHelper db)
    {
        _logger = logger;
        _db = db;
    }


    public SelectionSession StartSession(int projectId, string sessionName)
    {
        return _db.Query("""
                         INSERT INTO public.selection_session(project_id, name)
                         VALUES ($1, $2)
                         """, MapSelectionWithoutImages, projectId, sessionName
        );
    }

    public SelectionSession GetOrStartSession(int projectId, string sessionName)
    {
        return _db.Query("""
                         INSERT INTO public.selection_session(project_id, name)
                         VALUES ($1, $2)
                         ON CONFLICT (project_id) 
                         DO UPDATE SET name = $2
                         RETURNING id, project_id, name
                         """, MapSelectionWithoutImages, projectId, sessionName
        );
    }

    
    public void RemoveSession(int projectId)
    {
        _db.Execute("""
                    DELETE FROM public.selection_session
                    WHERE project_id = $1
                    """, projectId
        );
    }

    public void ClearSession(int projectId)
    {
        _db.Execute("""
                    DELETE FROM public.selection_session
                    WHERE project_id = $1
                    """, projectId);
    }

    public SelectionSession GetSessionById(int id)
    {
        return _db.Query("""
                         SELECT id, project_id, name FROM public.selection_session 
                         WHERE id = ($1)
                         """, MapSelection, id);
    }

    public int GetSessionIdByProjectId(int projectId)
    {
        return _db.Query("""
                         SELECT id FROM public.selection_session
                         WHERE project_id = $1
                         """, _db.MapToInt, projectId);
    }

    public bool ImageIsSelected(int sessionId, int imageId)
    {
        return _db.Query("""
                         SELECT EXISTS (
                             SELECT 1
                             FROM public.selection_session_image
                             WHERE selection_session_id = $1 
                             AND image_id = $2
                         );
                         """, _db.MapToBool, sessionId, imageId);
    }

    public void AddImageToProjectSelection(int selectionId, int imageId)
    {
        _db.Execute("""
                    INSERT INTO public.selection_session_image(selection_session_id, image_id)  
                    VALUES ($1, $2)
                    """, selectionId, imageId);
    }


    public void RemoveImageFromProjectSelection(int projectId, int imageId)
    {
        _db.Execute("""
                    DELETE FROM public.selection_session_image ssi
                    USING public.selection_session ss
                    WHERE ssi.image_id = ss.id
                    AND  ss.project_id = $1
                    AND  ssi.image_id = $2
                    """, projectId, imageId);
    }

    public SelectionSession GetByProject(int projectId)
    {
        return _db.Query(
            """
            SELECT ss.id, ss.project_id, ss.name,
            COALESCE(array_agg(ssi.image_id) FILTER (WHERE ssi.image_id IS NOT NULL), '{}') AS image_ids

            FROM public.selection_session ss
            LEFT JOIN public.selection_session_image ssi
            ON ss.id = ssi.selection_session_id
            WHERE ss.project_id = $1
            GROUP BY ss.id, ss.project_id, ss.name
            """, MapSelection, projectId);
    }

    public void DeleteExpiredSessions()
    {
        _db.Execute("""
                    DELETE FROM selection_session
                    WHERE created_at < now() - interval '1 hour';
                    """
        );
    }

    private static SelectionSession MapSelection(NpgsqlDataReader reader)
    {
        return new SelectionSession(
            (int)reader["id"],
            (int)reader["project_id"],
            (string)reader["name"],
            ((int[])reader["image_ids"]).ToList()
        );
    }

    private static SelectionSession MapSelectionWithoutImages(NpgsqlDataReader reader)
    {
        return new SelectionSession(
            (int)reader["id"],
            (int)reader["project_id"],
            (string)reader["name"],
            []
        );
    }

}