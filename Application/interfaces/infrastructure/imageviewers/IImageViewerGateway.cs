namespace Application.interfaces.imageviewers;

public interface IImageViewerGateway
{
    bool IsOpen();
    string? GetOpenedFile();
    void OpenFile(string imagePath);
}