using Application.interfaces;
using Application.services;
using Application.services.interfaces;
using Domain.entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Image = Domain.entities.Image;

namespace Tests.Application.services;

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

    private readonly string _projectPath = @"C:" + Path.DirectorySeparatorChar + "Projects" + Path.DirectorySeparatorChar + "Test";
    private readonly string _relativeFilePath = @"Original" + Path.DirectorySeparatorChar + "DSC_1234.JPG";
    private readonly string _originalPath = @"C:" + Path.DirectorySeparatorChar + "Projects" + Path.DirectorySeparatorChar + "Test" + Path.DirectorySeparatorChar + "Original" + Path.DirectorySeparatorChar + "DSC_1234.JPG";
    private readonly string _cacheRoot = @"C:" + Path.DirectorySeparatorChar + "ThumbnailCache";
    private readonly string _cacheDirectory = @"C:" + Path.DirectorySeparatorChar + "ThumbnailCache" + Path.DirectorySeparatorChar + "default";
    private readonly string _defaultCachePath = @"C:" + Path.DirectorySeparatorChar + "ThumbnailCache" + Path.DirectorySeparatorChar + "default" + Path.DirectorySeparatorChar + "5_300_q80.jpg";
    private readonly string _largeCachePath = @"C:" + Path.DirectorySeparatorChar + "ThumbnailCache" + Path.DirectorySeparatorChar + "large" + Path.DirectorySeparatorChar + "5_1200_q80.jpg";

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
            { "Thumbnails:CachePath", _cacheRoot }
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

    private Image CreateImage()
    {
        return new Image(
            5,
            10,
            null,
            "DSC_1234.JPG",
            ".JPG",
            _relativeFilePath
        );
    }

    private Project CreateProject()
    {
        return new Project(
            10,
            "Test project",
            _projectPath,
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
            .Setup(files => files.Combine(_projectPath, _relativeFilePath))
            .Returns(_originalPath);

        _files
            .Setup(files => files.Exists(_originalPath))
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
            .Setup(files => files.Combine(_projectPath, _relativeFilePath))
            .Returns(_originalPath);

        _files
            .Setup(files => files.Exists(_originalPath))
            .Returns(true);

        _files
            .Setup(files => files.Exists(_defaultCachePath))
            .Returns(true);

        _files
            .Setup(files => files.GetLastWriteTimeUtc(_originalPath))
            .Returns(originalModified);

        _files
            .Setup(files => files.GetLastWriteTimeUtc(_defaultCachePath))
            .Returns(cacheModified);

        // Execution
        ThumbnailResult result = _thumbnailService.GetThumbnail(5);

        // Asserts
        Assert.That(result.Found, Is.True);
        Assert.That(result.FilePath, Is.EqualTo(_defaultCachePath));
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
            .Setup(files => files.Combine(_projectPath, _relativeFilePath))
            .Returns(_originalPath);

        _files
            .Setup(files => files.Exists(_originalPath))
            .Returns(true);

        _files
            .Setup(files => files.Exists(_defaultCachePath))
            .Returns(false);

        _files
            .Setup(files => files.GetDirectoryName(_defaultCachePath))
            .Returns(_cacheDirectory);

        // Execution
        ThumbnailResult result = _thumbnailService.GetThumbnail(5);

        // Asserts
        Assert.That(result.Found, Is.True);
        Assert.That(result.FilePath, Is.EqualTo(_defaultCachePath));

        _files.Verify(
            files => files.FolderCreate(_cacheDirectory),
            Times.Once
        );

        _thumbnailGenerator.Verify(
            generator => generator.GenerateThumbnail(
                _originalPath,
                _defaultCachePath,
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
            .Setup(files => files.Combine(_projectPath, _relativeFilePath))
            .Returns(_originalPath);

        _files
            .Setup(files => files.Exists(_originalPath))
            .Returns(true);

        _files
            .Setup(files => files.Exists(_defaultCachePath))
            .Returns(true);

        _files
            .Setup(files => files.GetLastWriteTimeUtc(_originalPath))
            .Returns(originalModified);

        _files
            .Setup(files => files.GetLastWriteTimeUtc(_defaultCachePath))
            .Returns(cacheModified);

        _files
            .Setup(files => files.GetDirectoryName(_defaultCachePath))
            .Returns(_cacheDirectory);

        // Execution
        ThumbnailResult result = _thumbnailService.GetThumbnail(5);

        // Asserts
        Assert.That(result.Found, Is.True);
        Assert.That(result.FilePath, Is.EqualTo(_defaultCachePath));

        _files.Verify(
            files => files.FolderCreate(_cacheDirectory),
            Times.Once
        );

        _thumbnailGenerator.Verify(
            generator => generator.GenerateThumbnail(
                _originalPath,
                _defaultCachePath,
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

        string largeCacheDirectory = @"C:" + Path.DirectorySeparatorChar + "ThumbnailCache" + Path.DirectorySeparatorChar + "large";

        // Mocks
        _imageService
            .Setup(service => service.GetImageById(5))
            .Returns(image);

        _projectService
            .Setup(service => service.GetProjectById(10))
            .Returns(project);

        _files
            .Setup(files => files.Combine(_projectPath, _relativeFilePath))
            .Returns(_originalPath);

        _files
            .Setup(files => files.Exists(_originalPath))
            .Returns(true);

        _files
            .Setup(files => files.Exists(_largeCachePath))
            .Returns(false);

        _files
            .Setup(files => files.GetDirectoryName(_largeCachePath))
            .Returns(largeCacheDirectory);

        // Execution
        ThumbnailResult result = _thumbnailService.GetThumbnail(5, "large");

        // Asserts
        Assert.That(result.Found, Is.True);
        Assert.That(result.FilePath, Is.EqualTo(_largeCachePath));

        _thumbnailGenerator.Verify(
            generator => generator.GenerateThumbnail(
                _originalPath,
                _largeCachePath,
                1200,
                80u
            ),
            Times.Once
        );
    }
}