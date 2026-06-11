namespace Infrastructure.irfanview;

public interface IIrfanViewRepository
{
    bool IsOpen();
    string? GetOpenedFile();
}