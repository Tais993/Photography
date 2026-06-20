using Application.interfaces.website;
using Domain.entities;
using Domain.entities.search;
using Domain.website;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Website.WebsiteConstants;

namespace Website.Pages.Selection;

public class IndexModel : PageModel
{
    private readonly ISelectionIndexService _selectionIndexService;

    public IndexModel(ISelectionIndexService selectionIndexService)
    {
        _selectionIndexService = selectionIndexService;
    }

    public PaginatedResult<Image> ImagePage = PaginatedResult<Image>.Empty;

    public List<ImageViewModel> Images = [];

    public List<int> SelectedImageIds = [];

    public List<ProjectFolder> FolderOptions = [];

    public Project? SelectedProject { get; private set; }

    public SelectedImageViewModel? SelectedImage { get; private set; }

    public int ProjectImageCount { get; private set; }

    [BindProperty(SupportsGet = true)] public int? SelectedImageId { get; set; }
    [BindProperty(SupportsGet = true)] public int? SelectedProjectId { get; set; }
    
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public string? FolderName { get; set; }
    [BindProperty(SupportsGet = true)] public string? FileType { get; set; }

    [BindProperty(SupportsGet = true)] public int ImagePageSize { get; set; }
    [BindProperty(SupportsGet = true)] public int ImagePageNumber { get; set; }

    public PageResult OnGet()
    {
        SelectedProjectId ??= GetLastProjectIdFromCookie();

        SelectionIndexViewModel viewModel = _selectionIndexService.GetSelectionIndex(new SelectionIndexRequest
        {
            SelectedProjectId = SelectedProjectId,
            SelectedImageId = SelectedImageId,
            Search = Search,
            FolderName = FolderName,
            FileType = FileType,
            PageSize = ImagePageSize,
            PageNumber = ImagePageNumber
        });

        ImagePage = viewModel.ImagePage;
        Images = viewModel.Images;
        SelectedImageIds = viewModel.SelectedImageIds;
        FolderOptions = viewModel.FolderOptions;
        SelectedProject = viewModel.SelectedProject;
        SelectedImage = viewModel.SelectedImage;
        SelectedProjectId = viewModel.SelectedProjectId;
        SelectedImageId = viewModel.SelectedImageId;
        ProjectImageCount = viewModel.ProjectImageCount;
        ImagePageNumber = viewModel.PageNumber;
        ImagePageSize = viewModel.PageSize;

        return Page();
    }

    public IActionResult OnGetOpenImage()
    {
        if (SelectedProjectId is null || SelectedImageId is null)
        {
            return NotFound();
        }

        _selectionIndexService.OpenImageInImageViewer(
            SelectedProjectId.Value,
            SelectedImageId.Value);

        return new NoContentResult();
    }

    public IActionResult OnGetSelectedImage(int imageId)
    {
        SelectedImageViewModel? selectedImage = _selectionIndexService.GetSelectedImage(imageId);

        if (selectedImage is null)
        {
            return NotFound();
        }

        return Partial("_SelectedImage", selectedImage);
    }

    public IActionResult OnGetToggleImageSelection(int imageId, int selectedProjectId)
    {
        ImageViewModel? imageView = _selectionIndexService.ToggleImageSelection(
            imageId,
            selectedProjectId);

        if (imageView is null)
        {
            return NotFound();
        }

        return Partial("_ImageView", imageView);
    }

    private int? GetLastProjectIdFromCookie()
    {
        if (Request.Cookies.TryGetValue(LastProjectCookie, out string? projectIdText) &&
            int.TryParse(projectIdText, out int lastProjectId))
        {
            return lastProjectId;
        }

        return null;
    }
}