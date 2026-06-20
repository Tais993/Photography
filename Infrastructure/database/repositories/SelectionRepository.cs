using Application.interfaces.infrastructure;
using Domain.entities;
using Microsoft.Extensions.Logging;
using static Infrastructure.database.repositories.DatabaseMappers;

namespace Infrastructure.database.repositories;

public class SelectionRepository : ISelectionRepository
{
    private readonly RepositoryHelper _db;
    private readonly ILogger<SelectionRepository> _logger;

    public SelectionRepository(
        ILogger<SelectionRepository> logger,
        RepositoryHelper db)
    {
        _logger = logger;
        _db = db;
    }


    public SelectionSession StartSession(int projectId, string sessionName)
    {
        _logger.LogDebug("Starting selection session for project: {ProjectId}, session name: {SessionName}", projectId, sessionName);

        SelectionSession session = _db.Query("""
                                             INSERT INTO public.selection_session(project_id, name)
                                             VALUES ($1, $2)
                                             RETURNING id, project_id, name
                                             """, MapSelectionWithoutImages, projectId, sessionName
        );

        _logger.LogDebug("Started selection session: {SessionId} for project: {ProjectId}", session.Id, projectId);

        return session;
    }

    public SelectionSession GetOrStartSession(int projectId, string sessionName)
    {
        _logger.LogDebug("Getting or starting selection session for project: {ProjectId}, session name: {SessionName}", projectId, sessionName);

        SelectionSession session = _db.Query("""
                                             INSERT INTO public.selection_session(project_id, name)
                                             VALUES ($1, $2)
                                             ON CONFLICT (project_id) 
                                             DO UPDATE SET name = $2
                                             RETURNING id, project_id, name
                                             """, MapSelectionWithoutImages, projectId, sessionName
        );

        _logger.LogDebug("Got selection session: {SessionId} for project: {ProjectId}", session.Id, projectId);

        return session;
    }


    public void RemoveSession(int projectId)
    {
        _logger.LogDebug("Removing selection session for project: {ProjectId}", projectId);

        _db.Execute("""
                    DELETE FROM public.selection_session
                    WHERE project_id = $1
                    """, projectId
        );
    }

    public void ClearSession(int projectId)
    {
        _logger.LogDebug("Clearing selection session for project: {ProjectId}", projectId);

        _db.Execute("""
                    DELETE FROM public.selection_session_image
                    WHERE selection_session_id = (
                        SELECT id
                        FROM public.selection_session
                        WHERE project_id = $1
                    )
                    """, projectId);
    }

    public SelectionSession GetSessionById(int id)
    {
        _logger.LogDebug("Getting selection session by id: {SessionId}", id);

        return _db.Query("""
                         SELECT id, project_id, name FROM public.selection_session 
                         WHERE id = ($1)
                         """, MapSelection, id);
    }

    public int? GetSessionIdByProjectId(int projectId)
    {
        _logger.LogDebug("Getting selection session id for project: {ProjectId}", projectId);

        return _db.QueryScalarOrDefault<int>("""
                                    SELECT id FROM public.selection_session
                                    WHERE project_id = $1
                                    """, projectId);
    }

    public bool ImageIsSelected(int sessionId, int imageId)
    {
        _logger.LogDebug("Checking if image is selected, session: {SessionId}, image: {ImageId}", sessionId, imageId);

        bool isSelected = _db.QueryScalar<bool>("""
                                    SELECT EXISTS (
                                        SELECT 1
                                        FROM public.selection_session_image
                                        WHERE selection_session_id = $1 
                                        AND image_id = $2
                                    );
                                    """, sessionId, imageId);

        _logger.LogDebug("Image selected state is {IsSelected}, session: {SessionId}, image: {ImageId}", isSelected, sessionId, imageId);
        return isSelected;
    }

    public void AddImageToProjectSelection(int selectionId, int imageId)
    {
        _logger.LogDebug("Adding image to selection, session: {SessionId}, image: {ImageId}", selectionId, imageId);

        _db.Execute("""
                    INSERT INTO public.selection_session_image(selection_session_id, image_id)  
                    VALUES ($1, $2)
                    """, selectionId, imageId);
    }


    public void RemoveImageFromProjectSelection(int sessionId, int imageId)
    {
        _logger.LogDebug("Removing image from selection, session: {SessionId}, image: {ImageId}", sessionId, imageId);

        _db.Execute("""
                    DELETE FROM public.selection_session_image ssi
                    WHERE  ssi.selection_session_id = $1
                    AND  ssi.image_id = $2
                    """, sessionId, imageId);
    }

    public SelectionSession? GetByProject(int projectId)
    {
        _logger.LogDebug("Getting selection session for project: {ProjectId}", projectId);

        SelectionSession? session = _db.QueryOrDefault(
            """
            SELECT ss.id, ss.project_id, ss.name,
            COALESCE(array_agg(ssi.image_id) FILTER (WHERE ssi.image_id IS NOT NULL), '{}') AS image_ids

            FROM public.selection_session ss
            LEFT JOIN public.selection_session_image ssi
            ON ss.id = ssi.selection_session_id
            WHERE ss.project_id = $1
            GROUP BY ss.id, ss.project_id, ss.name
            """, MapSelection, projectId);


        if (session == null)
        {
            _logger.LogDebug("No selection session found for project: {ProjectId}", projectId);
            return null;
        }
        
        _logger.LogDebug(
            "Found selection session: {SessionId} for project: {ProjectId}, selected images: {SelectedImageCount}",
            session.Id,
            projectId,
            session.ImageIds.Count);

        return session;
    }

    public void DeleteExpiredSessions()
    {
        _logger.LogDebug("Deleting expired selection sessions");

        _db.Execute("""
                    DELETE FROM selection_session
                    WHERE created_at < now() - interval '1 hour';
                    """
        );
    }
}