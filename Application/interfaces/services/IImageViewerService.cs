namespace Application.interfaces.services;

public interface IImageViewerService
{
    bool IsAvailable();
    string GetImageViewerName();
    string? GetOpenedFileName(string? givenFileName);
    void OpenImage(string imagePath);
}