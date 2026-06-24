using System.Text.RegularExpressions;

namespace Application;

public class Constants
{
    public static readonly Regex ProjectNameRegex = new Regex("(\\d\\d\\d\\d)-(\\d{1,2})-(\\d{1,2})-([^.]*)");
    public static readonly Regex SubProjectNameRegex = new Regex("\\.([^.]*)");

    
    public const string ProjectInfoFile = "project.info";
    
    public const string ConfigImageViewerMode = "ImageViewer:Mode";
    public const string ConfigImageViewerPath = "ImageViewer:ImageViewerPath";
    public const string ConfigProjectFolder = "DefaultProjectFolder";

    public const string ConfigThumbnailsCachePath = "Thumbnails:CachePath";
    public const string ConfigThumbnailsDefaultSize = "Thumbnails:DefaultSize";
    public const string ConfigThumbnailsLargeSize = "Thumbnails:LargeSize";
    public const string ConfigThumbnailsJpegQuality = "Thumbnails:JpegQuality";
    
    public const string CollectionMetadataType = "Collection";
    public const string CollectionsConfigKey = "Collections";
    
    public const string FolderMetadataType = "Folder";
    public const string FolderNamesConfigKey = "FolderNames";
    public const string FolderMetadataKeyPrefix = "folder.";
    
    public const string OriginalsFolderMetadataKey = "folder.originals";
    public const string EditingFolderMetadataKey = "folder.editing";
    public const string FinalsFolderMetadataKey = "folder.finals";
    

    

    public static readonly string[] ImageFileTypesSql =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
    ];

    public static readonly HashSet<string> ImageFileTypes = new(
        ImageFileTypesSql,
        StringComparer.OrdinalIgnoreCase);
    
    public static readonly string[] RawFileTypesSql =
    [
        // Adobe
        ".dng",
        // Arri Alexa
        ".ari",
        // Blackmagic Design
        ".braw",
        // Canon
        ".crw",
        ".cr2",
        ".cr3",
        // Casio
        ".bay",
        // Cintel
        ".cri",
        // Epson
        ".erf",
        // Fujifilm
        ".raf",
        // GoPro
        ".gpr",
        // Hassalblad
        ".3fr",
        ".fff",
        // intoPIX
        ".tco",
        // JPEG
        ".jxs",
        // Kodak
        ".dcs",
        ".dcr",
        ".drf",
        ".k25",
        ".kdc",
        // Leaf
        ".mos",
        // Leica
        ".raw",
        ".rwl",
        // Logitech
        ".pxn",
        // Mamiya
        ".mef",
        // Minolta
        ".mdc",
        ".mrw",
        // Nikon
        ".nef",
        ".nrw",
        // Olympus
        ".orf",
        // Panasonic
        ".rw2",
        // Pentax
        ".pef",
        ".ptx",
        // Phase One
        ".cap",
        ".iiq",
        ".eip",
        // Rawzor
        ".rwz",
        // RED
        ".r3d",
        // Samsung
        ".srw",
        // Sigma
        ".x3f",
        // Sony
        ".arw",
        ".srf",
        ".sr2",

        // Not sure
        ".data",
        ".obm",
        ".tif",
    ];

    public static readonly HashSet<string> RawFileTypes = new(
        RawFileTypesSql,
        StringComparer.OrdinalIgnoreCase);
}