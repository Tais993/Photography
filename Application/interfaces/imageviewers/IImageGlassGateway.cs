namespace Application.interfaces.imageviewers;

public interface IImageGlassGateway
{
    bool IsOpen();
    string? GetOpenedFile();
    void OpenFile(string imagePath);
}