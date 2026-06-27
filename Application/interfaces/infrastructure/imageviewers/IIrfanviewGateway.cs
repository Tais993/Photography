namespace Application.interfaces.infrastructure.imageviewers;

public interface IIrfanviewGateway : IImageViewerGateway
{
    /// <summary>
    /// This opens the whole folder; depending on the image viewer used it will only display the JPGs.
    /// </summary>
    /// <param name="folderPath"></param>
    /// <param name="imageTypes"></param>
    void OpenFolder(string folderPath, HashSet<string> imageTypes);
}