namespace Application.interfaces.imageviewers;

public interface IWindowsPhotoGateway
{
    bool IsOpen();
    string? GetOpenedFile();
    void OpenFile(string imagePath);
}