using Domain.entities;

namespace Application.interfaces.infrastructure.repositories;

public interface IImageRepository
{
    public Image? GetById(int id);
    public List<Image> GetAll();
    public List<Image> GetAllByIds(int[] imageIds);
    public List<Image> GetAllByProjectId(int projectId);
    public List<Image> GetAllByProject(Project project);
    public Image Insert(Image image);
    public void Update(Image image);
    public void DeleteById(int id);

    int GetProjectImageCount(int projectId);
}