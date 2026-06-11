using Application.services.interfaces;
using Domain.entities;
using Domain.entities.search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Website.WebsiteConstants;

namespace Website.Pages.Projects;

public class IndexModel : PageModel
{
    private readonly IProjectService _projectService;
    private readonly ISearchService _searchService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IProjectService projectService, ISearchService searchService, ILogger<IndexModel> logger)
    {
        _projectService = projectService;
        _searchService = searchService;
        _logger = logger;
    }

    public List<Project> Projects { get; private set; } = [];

    public Project? SelectedProject { get; private set; }

    [BindProperty(SupportsGet = true)] public int? SelectedProjectId { get; set; }


    [BindProperty(SupportsGet = true)] public string? Search { get; set; }

    [BindProperty(SupportsGet = true)] public int? ProjectId { get; set; }
    [BindProperty(SupportsGet = true)] public int? ParentProjectId { get; set; }

    [BindProperty(SupportsGet = true)] public string? ProjectPath { get; set; }

    [BindProperty(SupportsGet = true)] public DateOnly? EventDate { get; set; }

    public void OnGet()
    {
        IEnumerable<Project> projects = _searchService.SearchProjects(new ProjectSearchSettings()
        {
            EventDate = this.EventDate,
            ProjectName = this.Search,
            ProjectPath = this.ProjectPath,
            ProjectId = this.ProjectId,
            ParentProjectId = this.ParentProjectId
        });
        
        _logger.LogInformation($"Found {projects.Count()} projects");

        Projects = projects
            .OrderByDescending(project => project.EventDate)
            .ThenBy(project => project.Name)
            .ToList();

        _logger.LogInformation($"Found {projects.Count()} projects");

        if (SelectedProjectId is not null)
        {
            SelectedProject = _projectService.GetProjectById(SelectedProjectId.Value);

            UpdateProjectCookie();
        }
        else if (Request.Cookies.TryGetValue(LastProjectCookie, out string? projectIdText) &&
                 int.TryParse(projectIdText, out int lastProjectId))
        {
            SelectedProjectId = lastProjectId;
            SelectedProject = _projectService.GetProjectById(lastProjectId);
        }
        else
        {
            SelectedProject = Projects.FirstOrDefault();
            SelectedProjectId = SelectedProject?.Id;
        }
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

    internal _ProjectView CreateProjectView(Project project)
    {
        return new _ProjectView
        {
            Selected = project.Id == SelectedProjectId,
            Id = project.Id,
            Name = project.Name,
            Path = project.Path,
            EventDate = project.EventDate
        };
    }

    public int GetProjectCount()
    {
        return _projectService.GetProjectCount();
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