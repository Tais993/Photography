using Application.interfaces.infrastructure;
using Application.interfaces.services;
using Application.interfaces.services.project;
using Application.interfaces.website;
using Domain.entities;
using Domain.entities.search;
using Domain.website;
using Microsoft.Extensions.Logging;

namespace Application.website;

public class SelectionIndexService : ISelectionIndexService
{
    private readonly IImageSelectionService _imageSelectionService;
    private readonly IProjectFolderService _folderService;
    private readonly ISearchService _searchService;
    private readonly IImageViewerService _imageViewerService;
    private readonly IProjectService _projectService;
    private readonly IProjectFolderService _projectFolderService;
    private readonly IImageService _imageService;
    private readonly IFiles _fileService;
    private readonly ILogger<SelectionIndexService> _logger;

    public SelectionIndexService(
        IImageSelectionService imageSelectionService,
        ISearchService searchService,
        IImageService imageService,
        IProjectService projectService,
        IProjectFolderService projectFolderService,
        IImageViewerService imageViewerService,
        IFiles fileService,
        ILogger<SelectionIndexService> logger, 
        IProjectFolderService folderService)
    {
        _imageSelectionService = imageSelectionService;
        _searchService = searchService;
        _imageService = imageService;
        _projectService = projectService;
        _projectFolderService = projectFolderService;
        _imageViewerService = imageViewerService;
        _fileService = fileService;
        _logger = logger;
        _folderService = folderService;
    }

    public SelectionIndexViewModel GetSelectionIndex(SelectionIndexRequest request)
    {
        _logger.LogDebug("Creating selection index view model for project: {ProjectId}", request.SelectedProjectId);

        SelectionIndexViewModel viewModel = new()
        {
            SelectedProjectId = request.SelectedProjectId,
            SelectedImageId = request.SelectedImageId,
            PageSize = request.PageSize,
            PageNumber = request.PageNumber
        };

        if (request.SelectedProjectId is null)
        {
            _logger.LogDebug("Selection index requested without selected project");
            return viewModel;
        }

        Project selectedProject = _projectService.GetProjectById(request.SelectedProjectId.Value);

        viewModel.SelectedProject = selectedProject;
        viewModel.ProjectImageCount = _imageService.GetProjectImageCount(request.SelectedProjectId.Value);
        viewModel.FolderOptions = _projectFolderService.GetExistingProjectFolders(request.SelectedProjectId.Value);

        string? folderName = GetVerifiedFolderName(
            request.FolderName,
            viewModel.FolderOptions);

        if (request.SelectedImageId is not null)
        {
            viewModel.SelectedImage = GetSelectedImage(request.SelectedImageId.Value);
        }

        _imageSelectionService.GetOrStartSession(selectedProject);

        viewModel.SelectedImageIds = _imageSelectionService
            .GetSessionImages(selectedProject)
            .ImageIds;

        _logger.LogInformation(
            "Selection session loaded for project: {ProjectId}, selected images: {SelectedImageCount}",
            request.SelectedProjectId,
            viewModel.SelectedImageIds.Count);

        ImageSearchSettings settings = new()
        {
            ProjectId = request.SelectedProjectId,
            FileNameOrNumber = request.Search,
            FolderName = folderName,
            FileType = request.FileType,
            HideRawFilesWhenImageExists = true,
            PageSize = request.PageSize,
            PageNumber = request.PageNumber
        };

        viewModel.ImagePage = _searchService.SearchImages(settings);
        viewModel.PageNumber = viewModel.ImagePage.PageNumber;
        viewModel.PageSize = viewModel.ImagePage.PageSize;

        viewModel.Images = viewModel.ImagePage.Items
            .Select(image => CreateImageViewModel(image, viewModel.SelectedImageIds))
            .ToList();

        _logger.LogInformation(
            "Created selection index for project: {ProjectId}, images on page: {Count}, selected images: {SelectedCount}",
            request.SelectedProjectId,
            viewModel.Images.Count,
            viewModel.SelectedImageIds.Count);

        return viewModel;
    }

    public SelectedImageViewModel? GetSelectedImage(int imageId)
    {
        _logger.LogDebug("Getting selected image view model for image: {ImageId}", imageId);

        Image? image = _imageService.GetImageById(imageId);

        if (image is null)
        {
            _logger.LogWarning("Selected image was not found: {ImageId}", imageId);
            return null;
        }

        return new SelectedImageViewModel
        {
            CanOpenInImageViewer = _imageViewerService.IsAvailable(),
            ImageViewerName = _imageViewerService.GetImageViewerName(),
            Image = image
        };
    }

    public ImageViewModel? ToggleImageSelection(int imageId, int selectedProjectId)
    {
        _logger.LogInformation("Toggling image selection from website, project: {ProjectId}, image: {ImageId}", selectedProjectId, imageId);

        Project? project = _projectService.GetProjectById(selectedProjectId);
        Image? image = _imageService.GetImageById(imageId);

        if (project is null || image is null)
        {
            _logger.LogWarning("Could not toggle image selection, project or image was not found. Project: {ProjectId}, image: {ImageId}", selectedProjectId, imageId);
            return null;
        }

        SelectionSession selectionSession = _imageSelectionService.GetOrStartSession(project);
        bool isSelected = _imageSelectionService.ToggleImageSelection((int) selectionSession.Id!, imageId);

        return CreateImageViewModel(image, isSelected);
    }

    public void OpenImageInImageViewer(int selectedProjectId, int selectedImageId)
    {
        _logger.LogInformation("Opening image in image viewer, project: {ProjectId}, image: {ImageId}", selectedProjectId, selectedImageId);

        Project selectedProject = _projectService.GetProjectById(selectedProjectId);
        Image selectedImage = _imageService.GetImageById(selectedImageId);

        string imagePath = _fileService.Combine(selectedProject.Path, selectedImage.RelationalFilePath);

        _imageViewerService.OpenImage(imagePath);
    }

    private static string? GetVerifiedFolderName(string? folderName, List<ProjectFolder> folderOptions)
    {
        if (string.IsNullOrWhiteSpace(folderName))
        {
            return null;
        }

        bool folderExists = folderOptions.Any(folder =>
            string.Equals(folder.FolderName, folderName, StringComparison.OrdinalIgnoreCase));

        if (!folderExists)
        {
            return null;
        }

        return folderName;
    }

    private static ImageViewModel CreateImageViewModel(Image image, List<int> selectedImageIds)
    {
        bool isSelected = image.Id is not null && selectedImageIds.Contains(image.Id.Value);

        return CreateImageViewModel(image, isSelected);
    }

    private static ImageViewModel CreateImageViewModel(Image image, bool isSelected)
    {
        return new ImageViewModel
        {
            Selected = isSelected,
            ImageId = image.Id,
            FileType = image.FileType,
            FileName = image.FileName,
            RelationalFilePath = image.RelationalFilePath
        };
    }
}