namespace Application.interfaces.imageviewers;

public interface IIrfanviewGateway
{
    bool IsOpen();
    string? GetOpenedFile();
    void OpenFile(string imagePath);
}