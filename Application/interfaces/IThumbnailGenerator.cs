namespace Application.interfaces;

public interface IThumbnailGenerator
{
    /// <summary>
    ///     Based on the original file's path, it writes a thumbnail version of the file to the given cachePath
    /// </summary>
    /// <param name="originalPath"></param>
    /// <param name="cachePath"></param>
    /// <param name="maxSize"></param>
    /// <param name="jpegQuality"></param>
    void GenerateThumbnail(string originalPath, string cachePath, int maxSize, uint jpegQuality);
}