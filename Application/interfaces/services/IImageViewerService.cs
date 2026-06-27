namespace Application.interfaces.services;

public interface IImageViewerService
{
    bool IsAvailable();
    string GetImageViewerName();
    string? GetOpenedFileName(string? givenFileName);
    void OpenImage(string imagePath);


    bool CanOpenFolders();

    /// <summary>
    ///     Allows opening of a given folder, and depending on the image viewer used will only display all non-raw images.
    /// </summary>
    /// <returns></returns>
    void OpenProjectFolder(string folderPath);
}