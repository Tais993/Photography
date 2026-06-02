using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Website.Pages.Projects;

public class _ProjectView : PageModel
{
    [BindProperty(SupportsGet = true)] public bool Selected { get; set; }
    [BindProperty(SupportsGet = true)] public int? Id { get; set;  }
    [BindProperty(SupportsGet = true)] public string? Name { get; set; }
    [BindProperty(SupportsGet = true)] public string? Path { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? EventDate { get; set; }

    public void OnGet()
    {
    }
}