namespace Application.services.interfaces;

public interface IFileSearchService
{
    public void searchImagesByName(string name);
    public void searchImagesByNumber(int number);
}