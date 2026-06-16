using Domain.website;

namespace Application.website.interfaces;

public interface ISelectionIndexService
{
    SelectionIndexViewModel GetSelectionIndex(SelectionIndexRequest request);
    SelectedImageViewModel? GetSelectedImage(int imageId);
    void OpenImageInImageViewer(int selectedProjectId, int selectedImageId);
    ImageViewModel? ToggleImageSelection(int imageId, int selectedProjectId);
}
