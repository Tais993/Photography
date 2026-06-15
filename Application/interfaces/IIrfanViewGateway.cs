namespace Application.interfaces;

public interface IIrfanViewGateway
{
    bool IsOpen();
    string? GetOpenedFile();
    void OpenFile(string filePath);
}