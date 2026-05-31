using Domain.entities;
using Infrastructure.database.repositories;
using Microsoft.Extensions.Logging;

namespace Application.services;

public class ImageSelectionService
{
    private readonly ILogger<ImageSelectionService> _logger;
    private readonly SelectionRepository _selectionRepository;
    private readonly IProjectRepository _projectRepository;

    // start session
    public ImageSelectionService(ILogger<ImageSelectionService> logger, SelectionRepository selectionRepository, IProjectRepository projectRepository)
    {
        _logger = logger;
        _selectionRepository = selectionRepository;
        _projectRepository = projectRepository;
    }

    public SelectionSession StartSession(Project project)
    {
        return StartSession((int) project.Id, project.Name);
    }

    public SelectionSession StartSession(int projectId, string? sessionName)
    {
        sessionName ??= _projectRepository.GetById(projectId).Name;

        return _selectionRepository.StartSession(projectId, sessionName);
    }

    public SelectionSession GetOrStartSession(Project project)
    {
        return GetOrStartSession((int) project.Id, project.Name);
    }

    public SelectionSession GetOrStartSession(int projectId, string? sessionName)
    {
        sessionName ??= _projectRepository.GetById(projectId).Name;
        
        return _selectionRepository.GetOrStartSession(projectId, sessionName);
    }

    public int GetSessionId(int projectId)
    {
        return _selectionRepository.GetSessionIdByProjectId(projectId);
    }
    
    public void RemoveSession(Project project)
    { 
        RemoveSession((int) project.Id);
    }

    public void RemoveSession(int projectId)
    {
        _selectionRepository.RemoveSession(projectId);
    }

    public void ClearSession(Project project)
    {
        ClearSession((int) project.Id);
    }

    public void ClearSession(int projectId)
    {
        _selectionRepository.ClearSession(projectId);
    }

    public void SelectImage(Image image)
    {
        AddImageToSelection(image.ProjectId, (int) image.Id);
    }

    public void AddImageToSelection(int sessionId, int imageId)
    {
        _selectionRepository.AddImageToProjectSelection(sessionId, imageId);
    }

    public void RemoveImageFromSelection(int sessionId, int imageId)
    {
        _selectionRepository.RemoveImageFromProjectSelection(sessionId, imageId);
    }

    public SelectionSession GetSessionImages(Project project)
    {
        return GetSessionImages((int) project.Id);
    }

    
    public SelectionSession GetSessionImages(int projectId)
    {
        return _selectionRepository.GetByProject(projectId);
    }

    public bool ImageIsSelected(int sessionId, int imageId)
    {
        return _selectionRepository.ImageIsSelected(sessionId, imageId);
    }
    
    public bool ToggleImageSelection(int sessionId, int imageId)
    {
        if (ImageIsSelected(sessionId, imageId))
        {
            RemoveImageFromSelection(sessionId, imageId);
            return false;
        }
        else
        {
            AddImageToSelection(sessionId, imageId);
            return true;
        }
    }
}