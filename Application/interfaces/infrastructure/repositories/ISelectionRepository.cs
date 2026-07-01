using Domain.entities;

namespace Application.interfaces.infrastructure.repositories;

public interface ISelectionRepository
{
    SelectionSession StartSession(int projectId, string sessionName);
    SelectionSession GetOrStartSession(int projectId, string sessionName);
    void RemoveSession(int projectId);
    void ClearSession(int projectId);
    SelectionSession? GetSessionById(int id);
    int? GetSessionIdByProjectId(int projectId);
    bool ImageIsSelected(int sessionId, int imageId);
    void AddImageToProjectSelection(int selectionId, int imageId);
    void RemoveImageFromProjectSelection(int sessionId, int imageId);
    SelectionSession? GetByProject(int projectId);
    void DeleteExpiredSessions();
}