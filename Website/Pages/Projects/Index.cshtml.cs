using Application.services.interfaces;
using Domain.entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Website.Constants;

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

    [BindProperty(SupportsGet = true)] public string? ProjectId { get; set; }

    [BindProperty(SupportsGet = true)] public string? ProjectPath { get; set; }

    [BindProperty(SupportsGet = true)] public DateOnly? EventDate { get; set; }

    public void OnGet()
    {
        IEnumerable<Project> projects = _searchService.SearchProjects(new ProjectSearchSettings()
        {
            EventDate = this.EventDate,
            ProjectName = this.Search,
            ProjectPath = this.ProjectPath,
            ProjectId = null
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
    
    public int GetProjectCount()
    {
        return _projectService.GetProjectCount();
    }
}