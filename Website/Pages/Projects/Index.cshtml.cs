using Application.services.interfaces;
using Domain.entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Website.Constants;

namespace Website.Pages.Projects;

public class IndexModel : PageModel
{
    private readonly IProjectService _projectService;

    public IndexModel(IProjectService projectService)
    {
        _projectService = projectService;
    }
    
    public List<Project> Projects { get; private set; } = [];

    public Project? SelectedProject { get; private set; }

    [BindProperty(SupportsGet = true)]
    public int? SelectedProjectId { get; set; }


    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }
    
    public void OnGet()
    {
     
        IEnumerable<Project> projects = _projectService.GetAllProjects();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            string search = Search.Trim();

            projects = projects.Where(project =>
                project.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || project.Path.Contains(search, StringComparison.OrdinalIgnoreCase)
                || project.EventDate.ToString().Contains(search, StringComparison.OrdinalIgnoreCase)
            );
        }
        
        Projects = projects
            .OrderByDescending(project => project.EventDate)
            .ThenBy(project => project.Name)
            .ToList();

        
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
        } else if (Request.Cookies.TryGetValue(LastProjectCookie, out string? projectIdText)
                  && int.TryParse(projectIdText, out int lastProjectId))
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
}