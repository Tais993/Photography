using Domain.entities;
using Domain.entities.search;

namespace Domain.website;

public class SelectionIndexViewModel
{
    public PaginatedResult<Image> ImagePage = PaginatedResult<Image>.Empty;
    public List<ImageViewModel> Images = [];
    public List<int> SelectedImageIds  = [];
    public List<ProjectFolder> FolderOptions = [];
    
    public Project? SelectedProject;
    public SelectedImageViewModel? SelectedImage;
    
    public int? SelectedProjectId;
    public int? SelectedImageId;
    public int ProjectImageCount;
    
    public int PageSize;
    public int PageNumber;
}