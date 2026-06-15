using Application.interfaces;
using Application.services;
using Application.services.imageviewers;
using Application.services.interfaces;
using Domain.entities;
using Domain.entities.search;
using Domain.website;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Application.Constants;
using static Website.WebsiteConstants;

namespace Website.Pages.Selection;

public class IndexModel : PageModel
{
    private readonly IImageSelectionService _imageSelectionService;
    private readonly ISearchService _searchService;
    private readonly IImageViewerService _imageViewerService;
    private readonly IProjectService _projectService;
    private readonly IImageService _imageService;
    private readonly ILogger<IndexModel> _logger;
    private readonly IFiles _fileService;

    public IndexModel(IImageSelectionService imageSelectionService, ISearchService searchService, IImageService imageService,
        IProjectService projectService, IImageViewerService imageViewerService, IFiles fileService, ILogger<IndexModel> logger)
    {
        _imageSelectionService = imageSelectionService;
        _searchService = searchService;
        _imageService = imageService;
        _imageViewerService = imageViewerService;
        _fileService = fileService;
        _logger = logger;
        _projectService = projectService;
    }

    public List<Image> Images = [];
    public List<int> SelectedImageIds = [];
    public Project? SelectedProject { get; private set; }
    public Image SelectedImage { get; private set; }

    [BindProperty(SupportsGet = true)] public int? SelectedImageId { get; set; }
    [BindProperty(SupportsGet = true)] public int? SelectedProjectId { get; set; }


    [BindProperty(SupportsGet = true)] public string? Search { get; set; }

    [BindProperty(SupportsGet = true)] public string? FolderName { get; set; }

    [BindProperty(SupportsGet = true)] public string? FileType { get; set; }

    public PageResult OnGet()
    {
        if (SelectedProjectId is not null)
        {
            SelectedProject = _projectService.GetProjectById((int)SelectedProjectId);
        }
        else if (Request.Cookies.TryGetValue(LastProjectCookie, out string? projectIdText) &&
                 int.TryParse(projectIdText, out int lastProjectId))
        {
            SelectedProjectId = lastProjectId;
            SelectedProject = _projectService.GetProjectById(lastProjectId);
        }
        else
        {
            Images = [];
            SelectedImageIds = [];
            return Page();
        }

        if (SelectedImageId is not null)
        {
            SelectedImage = _imageService.GetImageById(SelectedImageId.Value);
        }

        SelectionSession selectionSession = _imageSelectionService.GetOrStartSession(SelectedProject);
        SelectedImageIds = _imageSelectionService.GetSessionImages(SelectedProject).ImageIds;

        ImageSearchSettings settings = new()
        {
            ProjectId = SelectedProjectId,
            FileNameOrNumber = Search,
            FolderName = FolderName,
            FileType = FileType
        };

        Images = _searchService.SearchImages(settings);

        return Page();
    }

    public IEnumerable<Image> GetImages()
    {
        return _imageService.HideRawFilesWhenNonRawExists(Images);
    }


    private static bool IsRaw(string fileType)
    {
        return RawFileTypes.Contains(fileType);
    }

    public IActionResult OnGetOpenImage()
    {
        OpenImageInImageViewer();

        return new NoContentResult();
    }

    public IActionResult OnGetSelectedImage(int imageId)
    {
        Image? image = _imageService.GetImageById(imageId);
        SelectedImage = image;
        SelectedImageId = imageId;

        if (image is null)
        {
            return NotFound();
        }

        SelectedImageViewModel selectedImageModel = new()
        {
            CanOpenInImageViewer = _imageViewerService.IsAvailable(),
            ImageViewerName = _imageViewerService.GetImageViewerName(),
            Image = image
        };

        return Partial("_SelectedImage", selectedImageModel);
    }

    public PartialViewResult OnGetImage(Image image)
    {
        bool isSelected = SelectedImageIds.Contains(image.Id!.Value);

        _ImageView imageView = new _ImageView
        {
            Selected = isSelected,
            ImageId = (int)image.Id,
            FileType = image.FileType,
            FileName = image.FileName,
            RelationalFilePath = image.RelationalFilePath
        };

        return Partial("_ImageView", imageView);
    }

    public IActionResult OnGetToggleImageSelection(int imageId, int selectedProjectId)
    {
        _logger.LogInformation("OnPostToggleImageSelection");
        Project? project = _projectService.GetProjectById(selectedProjectId);
        Image? image = _imageService.GetImageById(imageId);

        if (project is null || image is null)
        {
            return NotFound();
        }

        int sessionId = _imageSelectionService.GetSessionId(selectedProjectId);
        bool isSelected = _imageSelectionService.ToggleImageSelection(sessionId, imageId);

        _ImageView imageView = new()
        {
            Selected = isSelected,
            ImageId = image.Id,
            FileType = image.FileType,
            FileName = image.FileName,
            RelationalFilePath = image.RelationalFilePath
        };

        _logger.LogInformation("ImageView");
        return Partial("_ImageView", imageView);
    }

    public void OpenImageInImageViewer()
    {
        SelectedProject = _projectService.GetProjectById((int)SelectedProjectId);
        SelectedImage = _imageService.GetImageById((int)SelectedImageId);

        string imagePath = _fileService.Combine(SelectedProject.Path, SelectedImage.RelationalFilePath);

        _imageViewerService.OpenImage(imagePath);
    }

    public int GetProjectImageCount()
    {
        if (SelectedProject is null)
        {
            return 0;
        }

        return _imageService.GetProjectImageCount((int)SelectedProjectId);
    }
}