namespace Application.services;

public interface IIrfanviewService
{
    string? GetFileName(string? givenFileName);
    void OpenImage(string irfanviewPath, string fullPath);
}