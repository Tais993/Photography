using Application.interfaces;
using Application.services;
using Domain.entities;
using Domain.entities.search;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Application.services;

[TestFixture]
public class SearchServiceTests
{
    private Mock<IImageRepository> _imageRepository = null!;
    private Mock<IProjectRepository> _projectRepository = null!;
    private Mock<ILogger<SearchService>> _logger = null!;
    private SearchService _searchService = null!;

    [SetUp]
    public void SetUp()
    {
        _imageRepository = new Mock<IImageRepository>();
        _projectRepository = new Mock<IProjectRepository>();
        _logger = new Mock<ILogger<SearchService>>();

        _searchService = new SearchService(
            _imageRepository.Object,
            _projectRepository.Object,
            _logger.Object
        );
    }

    [Test]
    public void SearchImages_ReturnsImages()
    {
        ImageSearchSettings settings = new()
        {
            ProjectId = 1,
            FileName = "DSC"
        };

        List<Image> expectedImages =
        [
            new Image(
                1,
                "DSC_1234.JPG",
                ".JPG",
                @"Original\DSC_1234.JPG"
            )
        ];

        // Mocks
        _imageRepository
            .Setup(r => r.SearchImages(settings))
            .Returns(expectedImages);

        // Execution
        List<Image> result = _searchService.SearchImages(settings).Items;

        // Asserts
        Assert.That(result, Is.EqualTo(expectedImages));
    }

    [Test]
    public void SearchImages_TrimsStringValues()
    {
        ImageSearchSettings settings = new()
        {
            FileName = "  DSC  ",
            FileNumber = "  1234  ",
            FolderName = "  Original  ",
            FileType = "  .JPG  "
        };

        // Mocks
        _imageRepository
            .Setup(r => r.SearchImages(It.IsAny<ImageSearchSettings>()))
            .Returns([]);

        // Execution
        _searchService.SearchImages(settings);

        // Asserts
        _imageRepository.Verify(r => r.SearchImages(
            It.Is<ImageSearchSettings>(s =>
                s.FileName == "DSC" &&
                s.FileNumber == "1234" &&
                s.FolderName == "Original" &&
                s.FileType == ".JPG"
            )
        ), Times.Once);
    }

    [Test]
    public void SearchImages_EmptyStringValuesBecomeNull()
    {
        ImageSearchSettings settings = new()
        {
            FileName = "",
            FileNumber = "   ",
            FolderName = "",
            FileType = "   "
        };

        // Mocks
        _imageRepository
            .Setup(r => r.SearchImages(It.IsAny<ImageSearchSettings>()))
            .Returns([]);

        // Execution
        _searchService.SearchImages(settings);

        // Asserts
        _imageRepository.Verify(r => r.SearchImages(
            It.Is<ImageSearchSettings>(s =>
                s.FileName == null &&
                s.FileNumber == null &&
                s.FolderName == null &&
                s.FileType == null
            )
        ), Times.Once);
    }

    [Test]
    public void SearchImages_FileNameOrNumberWithNumber_SetsFileNumber()
    {
        ImageSearchSettings settings = new()
        {
            FileNameOrNumber = "1234"
        };

        // Mocks
        _imageRepository
            .Setup(r => r.SearchImages(It.IsAny<ImageSearchSettings>()))
            .Returns([]);

        // Execution
        _searchService.SearchImages(settings);

        // Asserts
        _imageRepository.Verify(r => r.SearchImages(
            It.Is<ImageSearchSettings>(s =>
                s.FileNameOrNumber == "1234" &&
                s.FileNumber == "1234" &&
                s.FileName == null
            )
        ), Times.Once);
    }

    [Test]
    public void SearchImages_FileNameOrNumberWithText_SetsFileName()
    {
        ImageSearchSettings settings = new()
        {
            FileNameOrNumber = "DSC"
        };

        // Mocks
        _imageRepository
            .Setup(r => r.SearchImages(It.IsAny<ImageSearchSettings>()))
            .Returns([]);

        // Execution
        _searchService.SearchImages(settings);

        // Asserts
        _imageRepository.Verify(r => r.SearchImages(
            It.Is<ImageSearchSettings>(s =>
                s.FileNameOrNumber == "DSC" &&
                s.FileName == "DSC" &&
                s.FileNumber == null
            )
        ), Times.Once);
    }

    [Test]
    public void SearchImages_FileNameOrNumberIsTrimmedBeforeCheckingIfNumber()
    {
        ImageSearchSettings settings = new()
        {
            FileNameOrNumber = "  1234  "
        };

        // Mocks
        _imageRepository
            .Setup(r => r.SearchImages(It.IsAny<ImageSearchSettings>()))
            .Returns([]);

        // Execution
        _searchService.SearchImages(settings);

        // Asserts
        _imageRepository.Verify(r => r.SearchImages(
            It.Is<ImageSearchSettings>(s =>
                s.FileNameOrNumber == "1234" &&
                s.FileNumber == "1234" &&
                s.FileName == null
            )
        ), Times.Once);
    }

    [Test]
    public void SearchImages_EmptyFileNameOrNumber_DoesNotSetFileNameOrFileNumber()
    {
        ImageSearchSettings settings = new()
        {
            FileNameOrNumber = "   "
        };

        // Mocks
        _imageRepository
            .Setup(r => r.SearchImages(It.IsAny<ImageSearchSettings>()))
            .Returns([]);

        // Execution
        _searchService.SearchImages(settings);

        // Asserts
        _imageRepository.Verify(r => r.SearchImages(
            It.Is<ImageSearchSettings>(s =>
                s.FileNameOrNumber == null &&
                s.FileName == null &&
                s.FileNumber == null
            )
        ), Times.Once);
    }

    [Test]
    public void SearchProjects_ReturnsProjects()
    {
        ProjectSearchSettings settings = new()
        {
            ProjectName = "Holiday"
        };

        List<Project> expectedProjects =
        [
            new Project(
                1,
                "Holiday",
                @"C:\Photos\Holiday",
                new DateOnly(2024, 7, 4),
                null
            )
        ];

        // Mocks
        _projectRepository
            .Setup(r => r.SearchProjects(settings))
            .Returns(expectedProjects);

        // Execution
        List<Project> result = _searchService.SearchProjects(settings);

        // Asserts
        Assert.That(result, Is.EqualTo(expectedProjects));
    }

    [Test]
    public void SearchProjects_TrimsStringValues()
    {
        ProjectSearchSettings settings = new()
        {
            ProjectName = "  Holiday  ",
            ProjectPath = @"  C:\Photos\Holiday  "
        };

        // Mocks
        _projectRepository
            .Setup(r => r.SearchProjects(It.IsAny<ProjectSearchSettings>()))
            .Returns([]);

        // Execution
        _searchService.SearchProjects(settings);

        // Asserts
        _projectRepository.Verify(r => r.SearchProjects(
            It.Is<ProjectSearchSettings>(s =>
                s.ProjectName == "Holiday" &&
                s.ProjectPath == @"C:\Photos\Holiday"
            )
        ), Times.Once);
    }

    [Test]
    public void SearchProjects_EmptyStringValuesBecomeNull()
    {
        ProjectSearchSettings settings = new()
        {
            ProjectName = "",
            ProjectPath = "   "
        };

        // Mocks
        _projectRepository
            .Setup(r => r.SearchProjects(It.IsAny<ProjectSearchSettings>()))
            .Returns([]);

        // Execution
        _searchService.SearchProjects(settings);

        // Asserts
        _projectRepository.Verify(r => r.SearchProjects(
            It.Is<ProjectSearchSettings>(s =>
                s.ProjectName == null &&
                s.ProjectPath == null
            )
        ), Times.Once);
    }
    
    
        [Test]
    public void HideRawFilesWhenNonRawExists_HidesRawFile_WhenJpgWithSameNameExists()
    {
        List<Image> images =
        [
            new Image(
                1,
                "DSC_1234.NEF",
                ".NEF",
                @"Original\DSC_1234.NEF"
            ),
            new Image(
                2,
                "DSC_1234.JPG",
                ".JPG",
                @"Original\DSC_1234.JPG"
            )
        ];

        // Execution
        List<Image> result = _searchService
            .HideRawFilesWhenNonRawExists(images)
            .ToList();

        // Asserts
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].FileName, Is.EqualTo("DSC_1234.JPG"));
    }

    [Test]
    public void HideRawFilesWhenNonRawExists_KeepsRawFile_WhenNoNonRawWithSameNameExists()
    {
        List<Image> images =
        [
            new Image(
                1,
                "DSC_1234.NEF",
                ".NEF",
                @"Original\DSC_1234.NEF"
            )
        ];

        // Execution
        List<Image> result = _searchService
            .HideRawFilesWhenNonRawExists(images)
            .ToList();

        // Asserts
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].FileName, Is.EqualTo("DSC_1234.NEF"));
    }

    [Test]
    public void HideRawFilesWhenNonRawExists_KeepsNonRawFiles()
    {
        List<Image> images =
        [
            new Image(
                1,
                "DSC_1234.JPG",
                ".JPG",
                @"Original\DSC_1234.JPG"
            ),
            new Image(
                2,
                "DSC_1235.PNG",
                ".PNG",
                @"Original\DSC_1235.PNG"
            )
        ];

        // Execution
        List<Image> result = _searchService
            .HideRawFilesWhenNonRawExists(images)
            .ToList();

        // Asserts
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.Select(image => image.FileName), Does.Contain("DSC_1234.JPG"));
        Assert.That(result.Select(image => image.FileName), Does.Contain("DSC_1235.PNG"));
    }

    [Test]
    public void HideRawFilesWhenNonRawExists_ComparisonIsCaseInsensitive()
    {
        List<Image> images =
        [
            new Image(
                1,
                "DSC_1234.NEF",
                ".NEF",
                @"Original\DSC_1234.NEF"
            ),
            new Image(
                2,
                "dsc_1234.jpg",
                ".JPG",
                @"Original\dsc_1234.jpg"
            )
        ];

        // Execution
        List<Image> result = _searchService
            .HideRawFilesWhenNonRawExists(images)
            .ToList();

        // Asserts
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].FileName, Is.EqualTo("dsc_1234.jpg"));
    }
}