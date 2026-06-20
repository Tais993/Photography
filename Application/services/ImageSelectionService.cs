using Application.interfaces.infrastructure;
using Application.interfaces.services;
using Domain.entities;
using Microsoft.Extensions.Logging;

namespace Application.services;

public class ImageSelectionService : IImageSelectionService
{
    private readonly ILogger<IImageSelectionService> _logger;
    private readonly ISelectionRepository _selectionRepository;
    private readonly IProjectRepository _projectRepository;

    // start session
    public ImageSelectionService(ILogger<IImageSelectionService> logger, ISelectionRepository selectionRepository, IProjectRepository projectRepository)
    {
        _logger = logger;
        _selectionRepository = selectionRepository;
        _projectRepository = projectRepository;
    }

    public SelectionSession StartSession(Project project)
    {
        _logger.LogDebug("Starting selection session for project: {ProjectId}", project.Id);
        return StartSession((int) project.Id, project.Name);
    }

    public SelectionSession StartSession(int projectId, string? sessionName)
    {
        sessionName ??= _projectRepository.GetById(projectId).Name;

        _logger.LogInformation("Starting selection session for project: {ProjectId}, session name: {SessionName}", projectId, sessionName);
        return _selectionRepository.StartSession(projectId, sessionName);
    }

    public SelectionSession GetOrStartSession(Project project)
    {
        _logger.LogDebug("Getting or starting selection session for project: {ProjectId}", project.Id);
        return GetOrStartSession((int) project.Id, project.Name);
    }

    public SelectionSession GetOrStartSession(int projectId, string? sessionName)
    {
        sessionName ??= _projectRepository.GetById(projectId).Name;

        _logger.LogDebug("Getting or starting selection session for project: {ProjectId}, session name: {SessionName}", projectId, sessionName);
        return _selectionRepository.GetOrStartSession(projectId, sessionName);
    }

    public int? GetSessionId(int projectId)
    {
        _logger.LogDebug("Getting selection session id for project: {ProjectId}", projectId);
        return _selectionRepository.GetSessionIdByProjectId(projectId);
    }

    public void RemoveSession(Project project)
    { 
        RemoveSession((int) project.Id);
    }

    public void RemoveSession(int projectId)
    {
        _logger.LogInformation("Removing selection session for project: {ProjectId}", projectId);
        _selectionRepository.RemoveSession(projectId);
    }

    public void ClearSession(Project project)
    {
        ClearSession((int) project.Id);
    }

    public void ClearSession(int projectId)
    {
        _logger.LogInformation("Clearing selection session for project: {ProjectId}", projectId);
        _selectionRepository.ClearSession(projectId);
    }

    public void SelectImage(Image image)
    {
        _logger.LogDebug("Selecting image: {ImageId} for project: {ProjectId}", image.Id, image.ProjectId);
        AddImageToSelection(image.ProjectId, (int) image.Id);
    }

    public void AddImageToSelection(int sessionId, int imageId)
    {
        _logger.LogDebug("Adding image to selection, session: {SessionId}, image: {ImageId}", sessionId, imageId);
        _selectionRepository.AddImageToProjectSelection(sessionId, imageId);
    }

    public void RemoveImageFromSelection(int sessionId, int imageId)
    {
        _logger.LogDebug("Removing image from selection, session: {SessionId}, image: {ImageId}", sessionId, imageId);
        _selectionRepository.RemoveImageFromProjectSelection(sessionId, imageId);
    }

    public SelectionSession? GetSessionImages(Project project)
    {
        return GetSessionImages((int) project.Id);
    }


    public SelectionSession? GetSessionImages(int projectId)
    {
        _logger.LogDebug("Getting selected images for project: {ProjectId}", projectId);
        return _selectionRepository.GetByProject(projectId);
    }

    public bool ImageIsSelected(int sessionId, int imageId)
    {
        _logger.LogDebug("Checking if image is selected, session: {SessionId}, image: {ImageId}", sessionId, imageId);
        return _selectionRepository.ImageIsSelected(sessionId, imageId);
    }

    public bool ToggleImageSelection(int sessionId, int imageId)
    {
        _logger.LogDebug("Toggling image selection, session: {SessionId}, image: {ImageId}", sessionId, imageId);

        if (ImageIsSelected(sessionId, imageId))
        {
            RemoveImageFromSelection(sessionId, imageId);
            _logger.LogDebug("Image deselected, session: {SessionId}, image: {ImageId}", sessionId, imageId);
            return false;
        }
        else
        {
            AddImageToSelection(sessionId, imageId);
            _logger.LogDebug("Image selected, session: {SessionId}, image: {ImageId}", sessionId, imageId);
            return true;
        }
    }
}