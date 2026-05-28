using Domain.entities;

namespace Infrastructure.database.repositories;

public interface IImageRepository
{
    public Image GetById(int id);
    public List<Image> GetImagesByFileName(int projectId, string fileName);
    public List<Image> GetImagesByFileName(string fileName);
    public List<Image> GetImagesByPhotoNumber(int projectId, string fileNumber);
    public List<Image> GetImagesByPhotoNumber(string fileNumber);
    public List<Image> GetAll();
    public List<Image> GetAllByIds(int[] imageIds);
    public List<Image> GetAllByProjectId(int projectId);
    public List<Image> GetAllByProject(Project project);
    public Image Insert(Image image);
    public void Update(Image image);
    public void DeleteById(int id);

    public List<Image> SearchImages(ImageSearchSettings imageSearchSettings);
}