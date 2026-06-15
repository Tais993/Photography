using Domain.entities;

namespace Domain.website;

public class SelectedImageViewModel
{
    public Image Image { get; init; } = null!;
    public bool CanOpenInImageViewer { get; init; }
    public string ImageViewerName { get; init; } = "image viewer";
}