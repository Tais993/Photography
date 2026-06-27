using Domain.entities;

namespace Domain.website;

public class SelectedProjectViewModel
{
    public Project Project { get; init; }
    public bool CanOpenInImageViewer { get; init; }
    public string ImageViewerName { get; init; } = "image viewer";
}