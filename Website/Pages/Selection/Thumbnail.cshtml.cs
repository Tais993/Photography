using Application.interfaces.services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Website.Pages.Selection;

public class ThumbnailModel : PageModel
{
    private readonly IThumbnailService _thumbnailService;

    public ThumbnailModel(IThumbnailService thumbnailService)
    {
        _thumbnailService = thumbnailService;
    }

    public IActionResult OnGet(int imageId, string size = "default")
    {
        var thumbnail = _thumbnailService.GetThumbnail(imageId, size);

        if (!thumbnail.Found || thumbnail.FilePath is null)
        {
            return NotFound();
        }

        SetThumbnailCacheHeaders();

        return PhysicalFile(thumbnail.FilePath, thumbnail.ContentType);
    }
    
    private void SetThumbnailCacheHeaders()
    {
        Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        Response.Headers.Pragma = "no-cache";
        Response.Headers.Expires = "0";
    }
}