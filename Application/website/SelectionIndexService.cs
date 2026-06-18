using Application.interfaces;
using Application.services.imageviewers;
using Application.services.interfaces;
using Application.website.interfaces;
using Domain.entities;
using Domain.entities.search;
using Domain.website;

namespace Application.website;

public class SelectionIndexService : ISelectionIndexService
{
    private readonly IImageSelectionService _imageSelectionService;
    private readonly ISearchService _searchService;
    private readonly IImageViewerService _imageViewerService;
    private readonly IProjectService _projectService;
    private readonly IImageService _imageService;
    private readonly IFiles _fileService;

    public SelectionIndexService(
        IImageSelectionService imageSelectionService,
        ISearchService searchService,
        IImageService imageService,
        IProjectService projectService,
        IImageViewerService imageViewerService,
        IFiles fileService)
    {
        _imageSelectionService = imageSelectionService;
        _searchService = searchService;
        _imageService = imageService;
        _projectService = projectService;
        _imageViewerService = imageViewerService;
        _fileService = fileService;
    }

    public SelectionIndexViewModel GetSelectionIndex(SelectionIndexRequest request)
    {
        SelectionIndexViewModel viewModel = new()
        {
            SelectedProjectId = request.SelectedProjectId,
            SelectedImageId = request.SelectedImageId,
            PageSize = request.PageSize,
            PageNumber = request.PageNumber
        };

        if (request.SelectedProjectId is null)
        {
            return viewModel;
        }

        Project selectedProject = _projectService.GetProjectById(request.SelectedProjectId.Value);

        viewModel.SelectedProject = selectedProject;
        viewModel.ProjectImageCount = _imageService.GetProjectImageCount(request.SelectedProjectId.Value);

        if (request.SelectedImageId is not null)
        {
            viewModel.SelectedImage = GetSelectedImage(request.SelectedImageId.Value);
        }

        _imageSelectionService.GetOrStartSession(selectedProject);

        viewModel.SelectedImageIds = _imageSelectionService
            .GetSessionImages(selectedProject)
            .ImageIds;

        ImageSearchSettings settings = new()
        {
            ProjectId = request.SelectedProjectId,
            FileNameOrNumber = request.Search,
            FolderName = request.FolderName,
            FileType = request.FileType,
            HideRawImagesWhenJpgExists = true,
            PageSize = request.PageSize,
            PageNumber = request.PageNumber
        };

        viewModel.ImagePage = _searchService.SearchImages(settings);
        viewModel.PageNumber = viewModel.ImagePage.PageNumber;
        viewModel.PageSize = viewModel.ImagePage.PageSize;

        viewModel.Images = viewModel.ImagePage.Items
            .Select(image => CreateImageViewModel(image, viewModel.SelectedImageIds))
            .ToList();

        return viewModel;
    }

    public SelectedImageViewModel? GetSelectedImage(int imageId)
    {
        Image? image = _imageService.GetImageById(imageId);

        if (image is null)
        {
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
        Project? project = _projectService.GetProjectById(selectedProjectId);
        Image? image = _imageService.GetImageById(imageId);

        if (project is null || image is null)
        {
            return null;
        }

        int sessionId = _imageSelectionService.GetSessionId(selectedProjectId);
        bool isSelected = _imageSelectionService.ToggleImageSelection(sessionId, imageId);

        return CreateImageViewModel(image, isSelected);
    }

    public void OpenImageInImageViewer(int selectedProjectId, int selectedImageId)
    {
        Project selectedProject = _projectService.GetProjectById(selectedProjectId);
        Image selectedImage = _imageService.GetImageById(selectedImageId);

        string imagePath = _fileService.Combine(
            selectedProject.Path,
            selectedImage.RelationalFilePath);

        _imageViewerService.OpenImage(imagePath);
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