namespace Application.interfaces;

public interface IIrfanViewRepository
{
    bool IsOpen();
    string? GetOpenedFile();
}