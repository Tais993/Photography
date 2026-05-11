using Domain.entities;

namespace Application.services.interfaces;

public interface IFileSearchService
{
    public List<Image> searchImagesByName(string name);
    public List<Image> searchImagesByNumber(int number);
}