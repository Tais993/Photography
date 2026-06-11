using Application.interfaces;
using Application.services;
using Application.services.interfaces;
using Domain.entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Image = Domain.entities.Image;

namespace Tests.services;

[TestFixture]
[TestOf(typeof(ThumbnailService))]
public class ThumbnailServiceTest
{
    private Mock<IProjectService> _projectService = null!;
    private Mock<IImageService> _imageService = null!;
    private Mock<IFiles> _files = null!;
    private Mock<IThumbnailGenerator> _thumbnailGenerator = null!;
    private Mock<ILogger<ThumbnailService>> _logger = null!;
    private IConfiguration _configuration = null!;

    private ThumbnailService _thumbnailService = null!;

    private const string ProjectPath = @"C:\Projects\Test";
    private const string RelativeFilePath = @"Original\DSC_1234.JPG";
    private const string OriginalPath = @"C:\Projects\Test\Original\DSC_1234.JPG";
    private const string CacheRoot = @"C:\ThumbnailCache";
    private const string CacheDirectory = @"C:\ThumbnailCache\default";
    private const string DefaultCachePath = @"C:\ThumbnailCache\default\5_300_q80.jpg";
    private const string LargeCachePath = @"C:\ThumbnailCache\large\5_1200_q80.jpg";

    [SetUp]
    public void SetUp()
    {
        _projectService = new Mock<IProjectService>();
        _imageService = new Mock<IImageService>();
        _files = new Mock<IFiles>();
        _thumbnailGenerator = new Mock<IThumbnailGenerator>();
        _logger = new Mock<ILogger<ThumbnailService>>();

        Dictionary<string, string?> configurationValues = new()
        {
            { "Thumbnails:DefaultSize", "300" },
            { "Thumbnails:LargeSize", "1200" },
            { "Thumbnails:JpegQuality", "80" },
            { "Thumbnails:CachePath", CacheRoot }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();

        _thumbnailService = new ThumbnailService(
            _projectService.Object,
            _files.Object,
            _configuration,
            _logger.Object,
            _imageService.Object,
            _thumbnailGenerator.Object
        );
    }

    private static Image CreateImage()
    {
        return new Image(
            5,
            10,
            null,
            "DSC_1234.JPG",
            ".JPG",
            RelativeFilePath
        );
    }

    private static Project CreateProject()
    {
        return new Project(
            10,
            "Test project",
            ProjectPath,
            DateOnly.FromDateTime(DateTime.Today),
            null
        );
    }
    
    [Test]
    public void GetThumbnail_ReturnsNotFound_WhenImageDoesNotExist()
    {
        // Mocks
        _imageService
            .Setup(service => service.GetImageById(5))
            .Returns((Image?)null);

        // Execution
        ThumbnailResult result = _thumbnailService.GetThumbnail(5);

        // Asserts
        Assert.That(result.Found, Is.False);
        Assert.That(result.FilePath, Is.Null);

        _projectService.Verify(
            service => service.GetProjectById(It.IsAny<int>()),
            Times.Never
        );

        _thumbnailGenerator.Verify(
            generator => generator.GenerateThumbnail(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<uint>()
            ),
            Times.Never
        );
    }
    
    [Test]
    public void GetThumbnail_ReturnsNotFound_WhenProjectDoesNotExist()
    {
        Image image = CreateImage();

        // Mocks
        _imageService
            .Setup(service => service.GetImageById(5))
            .Returns(image);

        _projectService
            .Setup(service => service.GetProjectById(10))
            .Returns((Project?)null);

        // Execution
        ThumbnailResult result = _thumbnailService.GetThumbnail(5);

        // Asserts
        Assert.That(result.Found, Is.False);
        Assert.That(result.FilePath, Is.Null);

        _files.Verify(
            files => files.Combine(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );

        _thumbnailGenerator.Verify(
            generator => generator.GenerateThumbnail(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<uint>()
            ),
            Times.Never
        );
    }
    
    [Test]
    public void GetThumbnail_ReturnsNotFound_WhenOriginalFileDoesNotExist()
    {
        Image image = CreateImage();
        Project project = CreateProject();

        // Mocks
        _imageService
            .Setup(service => service.GetImageById(5))
            .Returns(image);

        _projectService
            .Setup(service => service.GetProjectById(10))
            .Returns(project);

        _files
            .Setup(files => files.Combine(ProjectPath, RelativeFilePath))
            .Returns(OriginalPath);

        _files
            .Setup(files => files.Exists(OriginalPath))
            .Returns(false);

        // Execution
        ThumbnailResult result = _thumbnailService.GetThumbnail(5);

        // Asserts
        Assert.That(result.Found, Is.False);
        Assert.That(result.FilePath, Is.Null);

        _thumbnailGenerator.Verify(
            generator => generator.GenerateThumbnail(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<uint>()
            ),
            Times.Never
        );
    }
    
    [Test]
    public void GetThumbnail_ReturnsExistingCachedThumbnail_WhenCacheIsValid()
    {
        Image image = CreateImage();
        Project project = CreateProject();

        DateTime originalModified = new DateTime(2026, 6, 11, 10, 0, 0, DateTimeKind.Utc);
        DateTime cacheModified = new DateTime(2026, 6, 11, 11, 0, 0, DateTimeKind.Utc);

        // Mocks
        _imageService
            .Setup(service => service.GetImageById(5))
            .Returns(image);

        _projectService
            .Setup(service => service.GetProjectById(10))
            .Returns(project);

        _files
            .Setup(files => files.Combine(ProjectPath, RelativeFilePath))
            .Returns(OriginalPath);

        _files
            .Setup(files => files.Exists(OriginalPath))
            .Returns(true);

        _files
            .Setup(files => files.Exists(DefaultCachePath))
            .Returns(true);

        _files
            .Setup(files => files.GetLastWriteTimeUtc(OriginalPath))
            .Returns(originalModified);

        _files
            .Setup(files => files.GetLastWriteTimeUtc(DefaultCachePath))
            .Returns(cacheModified);

        // Execution
        ThumbnailResult result = _thumbnailService.GetThumbnail(5);

        // Asserts
        Assert.That(result.Found, Is.True);
        Assert.That(result.FilePath, Is.EqualTo(DefaultCachePath));
        Assert.That(result.ContentType, Is.EqualTo("image/jpeg"));

        _files.Verify(
            files => files.FolderCreate(It.IsAny<string>()),
            Times.Never
        );

        _thumbnailGenerator.Verify(
            generator => generator.GenerateThumbnail(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<uint>()
            ),
            Times.Never
        );
    }
    
    [Test]
    public void GetThumbnail_GeneratesThumbnail_WhenCacheDoesNotExist()
    {
        Image image = CreateImage();
        Project project = CreateProject();

        // Mocks
        _imageService
            .Setup(service => service.GetImageById(5))
            .Returns(image);

        _projectService
            .Setup(service => service.GetProjectById(10))
            .Returns(project);

        _files
            .Setup(files => files.Combine(ProjectPath, RelativeFilePath))
            .Returns(OriginalPath);

        _files
            .Setup(files => files.Exists(OriginalPath))
            .Returns(true);

        _files
            .Setup(files => files.Exists(DefaultCachePath))
            .Returns(false);

        _files
            .Setup(files => files.GetDirectoryName(DefaultCachePath))
            .Returns(CacheDirectory);

        // Execution
        ThumbnailResult result = _thumbnailService.GetThumbnail(5);

        // Asserts
        Assert.That(result.Found, Is.True);
        Assert.That(result.FilePath, Is.EqualTo(DefaultCachePath));

        _files.Verify(
            files => files.FolderCreate(CacheDirectory),
            Times.Once
        );

        _thumbnailGenerator.Verify(
            generator => generator.GenerateThumbnail(
                OriginalPath,
                DefaultCachePath,
                300,
                80u
            ),
            Times.Once
        );
    }
    
    [Test]
    public void GetThumbnail_GeneratesThumbnail_WhenCacheIsOutdated()
    {
        Image image = CreateImage();
        Project project = CreateProject();

        DateTime originalModified = new DateTime(2026, 6, 11, 11, 0, 0, DateTimeKind.Utc);
        DateTime cacheModified = new DateTime(2026, 6, 11, 10, 0, 0, DateTimeKind.Utc);

        // Mocks
        _imageService
            .Setup(service => service.GetImageById(5))
            .Returns(image);

        _projectService
            .Setup(service => service.GetProjectById(10))
            .Returns(project);

        _files
            .Setup(files => files.Combine(ProjectPath, RelativeFilePath))
            .Returns(OriginalPath);

        _files
            .Setup(files => files.Exists(OriginalPath))
            .Returns(true);

        _files
            .Setup(files => files.Exists(DefaultCachePath))
            .Returns(true);

        _files
            .Setup(files => files.GetLastWriteTimeUtc(OriginalPath))
            .Returns(originalModified);

        _files
            .Setup(files => files.GetLastWriteTimeUtc(DefaultCachePath))
            .Returns(cacheModified);

        _files
            .Setup(files => files.GetDirectoryName(DefaultCachePath))
            .Returns(CacheDirectory);

        // Execution
        ThumbnailResult result = _thumbnailService.GetThumbnail(5);

        // Asserts
        Assert.That(result.Found, Is.True);
        Assert.That(result.FilePath, Is.EqualTo(DefaultCachePath));

        _files.Verify(
            files => files.FolderCreate(CacheDirectory),
            Times.Once
        );

        _thumbnailGenerator.Verify(
            generator => generator.GenerateThumbnail(
                OriginalPath,
                DefaultCachePath,
                300,
                80u
            ),
            Times.Once
        );
    }
    
    [Test]
    public void GetThumbnail_UsesLargeSize_WhenSizeIsLarge()
    {
        Image image = CreateImage();
        Project project = CreateProject();

        const string largeCacheDirectory = @"C:\ThumbnailCache\large";

        // Mocks
        _imageService
            .Setup(service => service.GetImageById(5))
            .Returns(image);

        _projectService
            .Setup(service => service.GetProjectById(10))
            .Returns(project);

        _files
            .Setup(files => files.Combine(ProjectPath, RelativeFilePath))
            .Returns(OriginalPath);

        _files
            .Setup(files => files.Exists(OriginalPath))
            .Returns(true);

        _files
            .Setup(files => files.Exists(LargeCachePath))
            .Returns(false);

        _files
            .Setup(files => files.GetDirectoryName(LargeCachePath))
            .Returns(largeCacheDirectory);

        // Execution
        ThumbnailResult result = _thumbnailService.GetThumbnail(5, "large");

        // Asserts
        Assert.That(result.Found, Is.True);
        Assert.That(result.FilePath, Is.EqualTo(LargeCachePath));

        _thumbnailGenerator.Verify(
            generator => generator.GenerateThumbnail(
                OriginalPath,
                LargeCachePath,
                1200,
                80u
            ),
            Times.Once
        );
    }
}