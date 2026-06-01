using Application.services;
using Application.services.interfaces;
using Domain.entities;
using Infrastructure.filesystem;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Application.Constants;
using static Website.WebsiteConstants;

namespace Website.Pages.Selection;

public class IndexModel : PageModel
{
    private readonly ImageSelectionService _imageSelectionService;
    private readonly ISearchService _searchService;
    private readonly IrfanviewService _irfanviewService;
    private readonly IProjectService _projectService;
    private readonly ILogger<IndexModel> _logger;
    private readonly IFiles _fileService;

    public IndexModel(ImageSelectionService imageSelectionService, ISearchService searchService,
        IProjectService projectService, IrfanviewService irfanviewService, IFiles fileService, ILogger<IndexModel> logger)
    {
        _imageSelectionService = imageSelectionService;
        _searchService = searchService;
        _irfanviewService = irfanviewService;
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

    public void OnGet()
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

        if (SelectedImageId is not null)
        {
            SelectedImage = _projectService.GetImageById(SelectedImageId.Value);
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
    }

    public IEnumerable<Image> HideNefFilesWhenJpgExists()
    {
        List<Image> imageList = Images.ToList();

        HashSet<string> nonRawKeys = imageList
            .Where(image => !IsRaw(image.FileType))
            .Select(image => image.FileName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);


        foreach (Image image in imageList)
        {
            if (SelectedImageIds.Contains((int)image.Id))
            {
                Console.WriteLine($"Image is selected: {image.FileName}");
            }

            Console.WriteLine($"Image is NOT selected: {image.FileName}, {image.Id}");
        }

        Console.WriteLine("Selected images:");

        foreach (int selectedImageId in SelectedImageIds)
        {
            Console.WriteLine($"Image: {selectedImageId}");
        }

        foreach (string nonRawKey in nonRawKeys)
        {
        }
        // afbeelding is RAW en heeft GEEN raw variant, dan is ie goedgekeurd
        // alle NIET raws

        return imageList.Where(image => !IsRaw(image.FileType) || !nonRawKeys.Contains(image.FileName));
    }


    private static bool IsRaw(string fileType)
    {
        return RawFileTypes.Contains(fileType);
    }

    public IActionResult OnGetOpenImage()
    {
        OpenImageInIrfanview();

        return new NoContentResult();
    }

    public IActionResult OnGetSelectedImage(int imageId)
    {
        Image? image = _projectService.GetImageById(imageId);
        SelectedImage = image;
        SelectedImageId = imageId;

        if (image is null)
        {
            return NotFound();
        }

        return Partial("_SelectedImage", image);
    }
    
    public PartialViewResult OnGetImage(Image image)
    {
        bool isSelected = SelectedImageIds.Contains(image.Id!.Value);

        _ImageView imageView = new _ImageView
        {
            Selected = isSelected,
            ImageId = (int) image.Id,
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
        Image? image = _projectService.GetImageById(imageId);

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

    public void OpenImageInIrfanview()
    {
        SelectedProject = _projectService.GetProjectById((int) SelectedProjectId);
        SelectedImage = _projectService.GetImageById((int) SelectedImageId);

        string imagePath = _fileService.Combine(SelectedProject.Path, SelectedImage.RelationalFilePath);

        _irfanviewService.OpenImage("C:\\Program Files\\IrfanView\\i_view64.exe", imagePath);
    }

    public int GetProjectImageCount()
    {
        return _projectService.GetProjectImageCount((int)SelectedProjectId);
    }
}