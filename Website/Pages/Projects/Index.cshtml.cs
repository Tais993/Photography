using Application.interfaces.services;
using Application.interfaces.services.project;
using Application.interfaces.website;
using Domain.entities;
using Domain.entities.search;
using Domain.website;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Website.WebsiteConstants;

namespace Website.Pages.Projects;

public class IndexModel : PageModel
{
    private const string ImportDriveRootPathKey = "CreateProjectModal.ImportDriveRootPath";
    private const string ProjectNameKey = "CreateProjectModal.ProjectName";
    private const string ProjectDateKey = "CreateProjectModal.ProjectDate";

    private readonly IProjectService _projectService;
    private readonly IProjectImportService _projectImportService;
    private readonly IProjectIndexService _projectIndexService;
    private readonly ICameraDriveService _cameraDriveService;

    public IndexModel(
        IProjectService projectService,
        IProjectIndexService projectIndexService,
        ICameraDriveService cameraDriveService,
        IProjectImportService projectImportService)
    {
        _projectService = projectService;
        _projectIndexService = projectIndexService;
        _cameraDriveService = cameraDriveService;
        _projectImportService = projectImportService;
    }

    public PaginatedResult<Project> ProjectPage = PaginatedResult<Project>.Empty;

    public List<Project> Projects { get; private set; } = [];
    public int ProjectCount { get; private set; }

    public Project? SelectedProject { get; private set; }

    public bool CanOpenProjectFolder { get; private set; }
    public string ImageViewerName { get; private set; } = "";

    [BindProperty] public CreateProjectModalModel CreateProjectModal { get; set; } = new();

    [BindProperty(SupportsGet = true)] public bool ShowCreateProjectModal { get; set; }

    [BindProperty(SupportsGet = true)] public int? SelectedProjectId { get; set; }

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int? ProjectId { get; set; }
    [BindProperty(SupportsGet = true)] public int? ParentProjectId { get; set; }
    [BindProperty(SupportsGet = true)] public string? ProjectPath { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? EventDate { get; set; }

    [BindProperty(SupportsGet = true)] public int ProjectPageNumber { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int ProjectPageSize { get; set; } = 60;

    [TempData] public bool ShowImportProgress { get; set; }
    [TempData] public Guid? ImportProgressId { get; set; }
    [TempData] public int ImportProgressTotalFiles { get; set; }

    public void OnGet()
    {
        LoadProjects();
        LoadCreateProjectModal();
    }

    #region Project creation

    public IActionResult OnPostSelectDrive()
    {
        LoadProjects();
        LoadImportDrives();

        LogicalDrive? importDrive = GetSelectedImportDrive();

        if (importDrive is null)
        {
            ModelState.AddModelError(ImportDriveRootPathKey, "Select an existing import drive.");
            OpenCreateProjectModal(importDrive, false);

            return Page();
        }

        CreateProjectModal.ProjectDate ??=
            DateOnly.FromDateTime(_cameraDriveService.GetFirstPhotoDate(importDrive) ?? DateTime.Today);

        OpenCreateProjectModal(importDrive, ModelState.IsValid);

        return Page();
    }

    public IActionResult OnPostBackToDrive()
    {
        LoadProjects();
        LoadCreateProjectModal();

        ShowCreateProjectModal = true;

        return Page();
    }

    public IActionResult OnPostCreateProject()
    {
        LoadProjects();
        LoadImportDrives();

        LogicalDrive? importDrive = GetSelectedImportDrive();

        ValidateCreateProject(importDrive);
        OpenCreateProjectModal(importDrive, true);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        Project project;

        try
        {
            List<string> files = _cameraDriveService.GetFilesToCopy(importDrive!);

            project = _projectService.CreateProject(
                CreateProjectModal.ProjectName!.Trim(),
                CreateProjectModal.ProjectDate!.Value);

            Guid importId = _projectImportService.StartImport(new ProjectImportRequest()
            {
                FilePaths = files,
                ProjectId = (int)project.Id!,
                RemoveSourceFilesAfterImport = true
            });

            PrepareImportProgress(importId, files);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred while creating the project.");

            Console.WriteLine(ex);

            return Page();
        }

        SelectedProjectId = project.Id;
        UpdateProjectCookie();

        return RedirectToPage("./Index", new
        {
            SelectedProjectId = project.Id
        });
    }

    public IActionResult OnGetImportProgress(Guid importId)
    {
        ProjectImportProgress? progress = _projectImportService.GetProgress(importId);

        if (progress is null)
        {
            return NotFound();
        }

        return new JsonResult(new
        {
            percentage = progress.Percentage,
            totalFiles = progress.TotalFiles,
            filesImported = progress.FilesImported,
            currentFile = progress.CurrentFile,
            isCompleted = progress.IsCompleted,
            hasFailed = progress.HasFailed,
            errorMessage = progress.ErrorMessage
        });
    }

    private void ValidateCreateProject(LogicalDrive? importDrive)
    {
        if (importDrive is null)
        {
            ModelState.AddModelError(ImportDriveRootPathKey, "Select an existing import drive.");
        }

        if (string.IsNullOrWhiteSpace(CreateProjectModal.ProjectName))
        {
            ModelState.AddModelError(ProjectNameKey, "Enter a project name.");
        }

        if (CreateProjectModal.ProjectDate is null)
        {
            ModelState.AddModelError(ProjectDateKey, "Enter a project date.");
        }
    }

    private void OpenCreateProjectModal(LogicalDrive? importDrive, bool isProjectDetailsStep)
    {
        CreateProjectModal.SelectedImportDrive = importDrive;
        CreateProjectModal.IsProjectDetailsStep = isProjectDetailsStep;
        ShowCreateProjectModal = true;
    }

    private void PrepareImportProgress(Guid importId, List<string> files)
    {
        ShowImportProgress = true;
        ImportProgressId = importId;
        ImportProgressTotalFiles = files.Count;
    }

    #endregion

    public IActionResult OnGetSelectedProjectView(int projectId)
    {
        Project? project = _projectService.GetProjectById(projectId);
        SelectedProject = project;
        SelectedProjectId = projectId;

        if (project is null)
        {
            return NotFound();
        }

        UpdateProjectCookie();

        return Partial("_SelectedProjectView", CreateSelectedProjectView(project));
    }

    public IActionResult OnGetProjectView(int projectId, int selectedProjectId)
    {
        Project? project = _projectService.GetProjectById(projectId);

        if (project is null)
        {
            return NotFound();
        }

        SelectedProjectId = selectedProjectId;
        SelectedProject = project;

        return Partial("_ProjectView", CreateProjectView(project));
    }

    public IActionResult OnGetOpenProjectFolder(int selectedProjectId)
    {
        _projectIndexService.OpenProjectFolder(selectedProjectId);

        return new NoContentResult();
    }

    internal ProjectViewModel CreateProjectView(Project project)
    {
        return _projectIndexService.CreateProjectView(project, SelectedProjectId);
    }

    internal SelectedProjectViewModel CreateSelectedProjectView(Project project)
    {
        return _projectIndexService.CreateSelectedProjectView(project);
    }

    private void LoadProjects()
    {
        SelectedProjectId ??= GetLastProjectIdFromCookie();

        ProjectIndexViewModel viewModel = _projectIndexService.GetProjectIndex(new ProjectIndexRequest()
        {
            SelectedProjectId = SelectedProjectId,
            Search = Search,
            ProjectId = ProjectId,
            ParentProjectId = ParentProjectId,
            ProjectPath = ProjectPath,
            EventDate = EventDate,
            ProjectPageNumber = ProjectPageNumber,
            ProjectPageSize = ProjectPageSize
        });

        ProjectPage = viewModel.ProjectPage;
        Projects = viewModel.Projects;
        SelectedProject = viewModel.SelectedProject;
        SelectedProjectId = viewModel.SelectedProjectId;
        ProjectCount = viewModel.ProjectCount;
        ProjectPageNumber = viewModel.ProjectPageNumber;
        ProjectPageSize = viewModel.ProjectPageSize;
        CanOpenProjectFolder = viewModel.CanOpenProjectInImageViewer;
        ImageViewerName = viewModel.ImageViewerName;

        if (SelectedProjectId is not null)
        {
            UpdateProjectCookie();
        }
    }

    private void LoadCreateProjectModal()
    {
        LogicalDrive? selectedImportDrive = _cameraDriveService.GetImportDrives().FirstOrDefault(drive => drive.HasDcimFolder);
        
        CreateProjectModal = new CreateProjectModalModel()
        {
            ImportDrives = _cameraDriveService.GetImportDrives(),
            SelectedImportDrive = selectedImportDrive,
            ImportDriveRootPath = selectedImportDrive?.RootPath,    
            ProjectDate = DateOnly.FromDateTime(DateTime.Today),
            IsProjectDetailsStep = false
        };
    }

    private void LoadImportDrives()
    {
        CreateProjectModal.ImportDrives = _cameraDriveService.GetImportDrives();
    }

    private LogicalDrive? GetSelectedImportDrive()
    {
        if (string.IsNullOrWhiteSpace(CreateProjectModal.ImportDriveRootPath))
        {
            return null;
        }

        return CreateProjectModal.ImportDrives.FirstOrDefault(drive =>
            string.Equals(
                drive.RootPath,
                CreateProjectModal.ImportDriveRootPath,
                StringComparison.OrdinalIgnoreCase));
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

    private void UpdateProjectCookie()
    {
        if (SelectedProjectId is null)
        {
            return;
        }

        Response.Cookies.Append(
            LastProjectCookie,
            SelectedProjectId.Value.ToString(),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true,
                SameSite = SameSiteMode.Lax
            });
    }
}