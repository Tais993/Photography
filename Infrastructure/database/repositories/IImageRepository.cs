using Domain.entities;

namespace Infrastructure.database.repositories;

public interface IImageRepository
{
    public Image GetByKey(int id);
    public List<Image> GetAll();
    public List<Image> GetAllByProjectId(int projectId);
    public List<Image> GetAllByProject(Project project);
    public Image Insert(Image image);
    public void Update(Image image);
    public void DeleteByKey(int id);

}