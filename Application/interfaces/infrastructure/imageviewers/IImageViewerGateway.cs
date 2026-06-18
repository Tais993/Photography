namespace Application.interfaces.infrastructure.imageviewers;

public interface IImageViewerGateway
{
    bool IsOpen();
    string? GetOpenedFile();
    void OpenFile(string imagePath);
}