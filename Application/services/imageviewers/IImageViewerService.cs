namespace Application.services.imageviewers;

public interface IImageViewerService
{
    bool IsAvailable();
    string? GetOpenedFileName(string? givenFileName);
    void OpenImage(string imagePath);
}