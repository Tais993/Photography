using Domain.entities;

namespace Application.interfaces.services;

public interface IImageSelectionService
{
    SelectionSession StartSession(Project project);
    SelectionSession StartSession(int projectId, string? sessionName);
    SelectionSession GetOrStartSession(Project project);
    SelectionSession GetOrStartSession(int projectId, string? sessionName);
    int? GetSessionId(int projectId);
    void RemoveSession(Project project);
    void RemoveSession(int projectId);
    void ClearSession(Project project);
    void ClearSession(int projectId);
    void SelectImage(Image image);
    void AddImageToSelection(int sessionId, int imageId);
    void RemoveImageFromSelection(int sessionId, int imageId);
    SelectionSession? GetSessionImages(Project project);
    SelectionSession? GetSessionImages(int projectId);
    bool ImageIsSelected(int sessionId, int imageId);
    bool ToggleImageSelection(int sessionId, int imageId);
}