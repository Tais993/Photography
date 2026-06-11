namespace Application.services.interfaces;

public interface IIrfanViewService
{
    string? GetFileName(string? givenFileName);
    void OpenImage(string fullPath);
}