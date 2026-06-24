using Application.interfaces.infrastructure;
using Application.interfaces.services;
using Application.interfaces.services.project;
using Application.services;
using Domain.entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Image = Domain.entities.Image;

namespace Tests.Unit.Application.services;

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

    private string _testRoot = null!;
    private string _projectPath = null!;
    private string _relativeFilePath = null!;
    private string _originalPath = null!;
    private string _cacheRoot = null!;
    private string _cacheDirectory = null!;
    private string _defaultCachePath = null!;
    private string _largeCachePath = null!;

    [SetUp]
    public void SetUp()
    {
        _projectService = new Mock<IProjectService>();
        _imageService = new Mock<IImageService>();
        _files = new Mock<IFiles>();
        _thumbnailGenerator = new Mock<IThumbnailGenerator>();
        _logger = new Mock<ILogger<ThumbnailService>>();

        _testRoot = Path.Combine(Path.GetTempPath(), "PictureProjectTests", Guid.NewGuid().ToString());

        _projectPath = Path.Combine(_testRoot, "Projects", "Test");

        _relativeFilePath = Path.Combine("Original", "DSC_1234.JPG");

        _originalPath = Path.Combine(
            _projectPath,
            "Original",
            "DSC_1234.JPG");

        _cacheRoot = Path.Combine(_testRoot, "ThumbnailCache");

        _cacheDirectory = Path.Combine(
            _cacheRoot,
            "default");

        _defaultCachePath = Path.Combine(
            _cacheRoot,
            "default",
            "5_300_q80.jpg");

        _largeCachePath = Path.Combine(
            _cacheRoot,
            "large",
            "5_1200_q80.jpg");

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

        _files
            .Setup(files => files.Combine(_cacheRoot, "default", "5_300_q80.jpg"))
            .Returns(_defaultCachePath);

        _files
            .Setup(files => files.Combine(_cacheRoot, "large", "5_1200_q80.jpg"))
            .Returns(_largeCachePath);

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
            10,
            "DSC_1234.JPG",
            ".JPG",
            _relativeFilePath,
            5
        );
    }

    private Project CreateProject()
    {
        return new Project(
            "Test project",
            _projectPath,
            DateOnly.FromDateTime(DateTime.Today),
            10
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

        string largeCacheDirectory = Path.Combine(_cacheRoot, "large");

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