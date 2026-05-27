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
        // no
        SelectImage(image.ProjectId, (int) image.Id);
    }

    public void SelectImage(int selectionId, int imageId)
    {
        _selectionRepository.AddImageToProjectSelection(selectionId, imageId);
    }

    public SelectionSession GetSessionImages(Project project)
    {
        return GetSessionImages((int) project.Id);
    }

    
    public SelectionSession GetSessionImages(int projectId)
    {
        return _selectionRepository.GetByProject(projectId);
    }
}