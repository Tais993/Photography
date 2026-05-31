using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Website.Pages.Selection;

public class _ImageView : PageModel
{
    
    [BindProperty(SupportsGet = true)] public bool Selected { get; set; }
    [BindProperty(SupportsGet = true)] public int? ImageId { get; set; }
    [BindProperty(SupportsGet = true)] public string? FileType { get; set; }
    [BindProperty(SupportsGet = true)] public string? FileName { get; set; }
    [BindProperty(SupportsGet = true)] public string? RelationalFilePath { get; set; }


    public void OnGet()
    {
        
    }
}