namespace Application.services.interfaces;

public interface IIrfanviewService
{
    string? GetFileName(string? givenFileName);
    void OpenImage(string irfanviewPath, string fullPath);
}