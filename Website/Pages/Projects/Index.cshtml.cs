using Application.interfaces.services;
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
    private readonly IProjectService _projectService;
    private readonly IProjectIndexService _projectIndexService;

    public IndexModel(IProjectService projectService, IProjectIndexService projectIndexService)
    {
        _projectService = projectService;
        _projectIndexService = projectIndexService;
    }

    public PaginatedResult<Project> ProjectPage = PaginatedResult<Project>.Empty;

    public List<Project> Projects { get; private set; } = [];

    public Project? SelectedProject { get; private set; }

    public int ProjectCount { get; private set; }

    [BindProperty(SupportsGet = true)] public int? SelectedProjectId { get; set; }

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int? ProjectId { get; set; }
    [BindProperty(SupportsGet = true)] public int? ParentProjectId { get; set; }
    [BindProperty(SupportsGet = true)] public string? ProjectPath { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? EventDate { get; set; }

    [BindProperty(SupportsGet = true)] public int ProjectPageNumber { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int ProjectPageSize { get; set; } = 60;

    public void OnGet()
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

        if (SelectedProjectId is not null)
        {
            UpdateProjectCookie();
        }
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

    internal ProjectViewModel CreateProjectView(Project project)
    {
        return _projectIndexService.CreateProjectView(project, SelectedProjectId);
    }

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

        return Partial("_SelectedProjectView", project);
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
}