using Application.interfaces.infrastructure;
using ImageMagick;

namespace Infrastructure.images;

public class MagickThumbnailGenerator : IThumbnailGenerator
{
    /// <summary>
    /// Based on the original file's path, it writes a thumbnail version of the file to the given cachePath
    /// </summary>
    /// <param name="originalPath"></param>
    /// <param name="cachePath"></param>
    /// <param name="maxSize"></param>
    /// <param name="jpegQuality"></param>
    public void GenerateThumbnail(string originalPath, string cachePath, int maxSize, uint jpegQuality)
    {
        uint thumbnailSize = (uint)maxSize;

        using MagickImage imageFile = new(originalPath);

        imageFile.AutoOrient();

        imageFile.Resize(new MagickGeometry(thumbnailSize, thumbnailSize)
        {
            IgnoreAspectRatio = false
        });

        imageFile.Format = MagickFormat.Jpeg;
        imageFile.Quality = jpegQuality;

        imageFile.Write(cachePath);
    }
}