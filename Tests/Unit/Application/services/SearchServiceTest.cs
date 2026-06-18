using Application.interfaces.infrastructure;
using Application.services;
using Domain.entities;
using Domain.entities.search;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Unit.Application.services;

[TestFixture]
public class SearchServiceTests
{
    private Mock<ISearchRepository> _searchRepository = null!;
    private Mock<ILogger<SearchService>> _logger = null!;
    private SearchService _searchService = null!;

    [SetUp]
    public void SetUp()
    {
        _searchRepository = new Mock<ISearchRepository>();
        _logger = new Mock<ILogger<SearchService>>();

        _searchService = new SearchService(
            _searchRepository.Object,
            _logger.Object
        );
    }

    [Test]
    public void SearchImages_ReturnsPaginatedResult()
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
                "DSC_1234",
                ".JPG",
                @"Original\DSC_1234.JPG"
            )
        ];

        // Mocks
        _searchRepository
            .Setup(repository => repository.CountImages(settings))
            .Returns(1);

        _searchRepository
            .Setup(repository => repository.SearchImages(settings))
            .Returns(expectedImages);

        // Execution
        PaginatedResult<Image> result = _searchService.SearchImages(settings);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Items, Is.EqualTo(expectedImages));
            Assert.That(result.TotalItems, Is.EqualTo(1));
            Assert.That(result.PageNumber, Is.EqualTo(1));
            Assert.That(result.PageSize, Is.EqualTo(60));
        }
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
        _searchRepository
            .Setup(repository => repository.CountImages(It.IsAny<ImageSearchSettings>()))
            .Returns(0);

        _searchRepository
            .Setup(repository => repository.SearchImages(It.IsAny<ImageSearchSettings>()))
            .Returns([]);

        // Execution
        _searchService.SearchImages(settings);

        // Asserts
        _searchRepository.Verify(repository => repository.CountImages(
            It.Is<ImageSearchSettings>(searchSettings =>
                searchSettings.FileName == "DSC" &&
                searchSettings.FileNumber == "1234" &&
                searchSettings.FolderName == "Original" &&
                searchSettings.FileType == ".JPG"
            )
        ), Times.Once);

        _searchRepository.Verify(repository => repository.SearchImages(
            It.Is<ImageSearchSettings>(searchSettings =>
                searchSettings.FileName == "DSC" &&
                searchSettings.FileNumber == "1234" &&
                searchSettings.FolderName == "Original" &&
                searchSettings.FileType == ".JPG"
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
        _searchRepository
            .Setup(repository => repository.CountImages(It.IsAny<ImageSearchSettings>()))
            .Returns(0);

        _searchRepository
            .Setup(repository => repository.SearchImages(It.IsAny<ImageSearchSettings>()))
            .Returns([]);

        // Execution
        _searchService.SearchImages(settings);

        // Asserts
        _searchRepository.Verify(repository => repository.SearchImages(
            It.Is<ImageSearchSettings>(searchSettings =>
                searchSettings.FileName == null &&
                searchSettings.FileNumber == null &&
                searchSettings.FolderName == null &&
                searchSettings.FileType == null
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
        _searchRepository
            .Setup(repository => repository.CountImages(It.IsAny<ImageSearchSettings>()))
            .Returns(0);

        _searchRepository
            .Setup(repository => repository.SearchImages(It.IsAny<ImageSearchSettings>()))
            .Returns([]);

        // Execution
        _searchService.SearchImages(settings);

        // Asserts
        _searchRepository.Verify(repository => repository.SearchImages(
            It.Is<ImageSearchSettings>(searchSettings =>
                searchSettings.FileNameOrNumber == "1234" &&
                searchSettings.FileNumber == "1234" &&
                searchSettings.FileName == null
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
        _searchRepository
            .Setup(repository => repository.CountImages(It.IsAny<ImageSearchSettings>()))
            .Returns(0);

        _searchRepository
            .Setup(repository => repository.SearchImages(It.IsAny<ImageSearchSettings>()))
            .Returns([]);

        // Execution
        _searchService.SearchImages(settings);

        // Asserts
        _searchRepository.Verify(repository => repository.SearchImages(
            It.Is<ImageSearchSettings>(searchSettings =>
                searchSettings.FileNameOrNumber == "DSC" &&
                searchSettings.FileName == "DSC" &&
                searchSettings.FileNumber == null
            )
        ), Times.Once);
    }

    [Test]
    public void SearchImages_InvalidPagination_UsesDefaults()
    {
        ImageSearchSettings settings = new()
        {
            PageNumber = 0,
            PageSize = 0
        };

        // Mocks
        _searchRepository
            .Setup(repository => repository.CountImages(It.IsAny<ImageSearchSettings>()))
            .Returns(0);

        _searchRepository
            .Setup(repository => repository.SearchImages(It.IsAny<ImageSearchSettings>()))
            .Returns([]);

        // Execution
        PaginatedResult<Image> result = _searchService.SearchImages(settings);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.PageNumber, Is.EqualTo(1));
            Assert.That(result.PageSize, Is.EqualTo(20));
        }
    }

    [Test]
    public void SearchImages_PageNumberAboveTotalPages_UsesLastPage()
    {
        ImageSearchSettings settings = new()
        {
            PageNumber = 10,
            PageSize = 20
        };

        // Mocks
        _searchRepository
            .Setup(repository => repository.CountImages(It.IsAny<ImageSearchSettings>()))
            .Returns(45);

        _searchRepository
            .Setup(repository => repository.SearchImages(It.IsAny<ImageSearchSettings>()))
            .Returns([]);

        // Execution
        PaginatedResult<Image> result = _searchService.SearchImages(settings);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.PageNumber, Is.EqualTo(3));

            _searchRepository.Verify(repository => repository.SearchImages(
                It.Is<ImageSearchSettings>(searchSettings =>
                    searchSettings.PageNumber == 3 &&
                    searchSettings.PageSize == 20
                )
            ), Times.Once);
        }
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
        _searchRepository
            .Setup(repository => repository.CountProjects(It.IsAny<ProjectSearchSettings>()))
            .Returns(0);

        _searchRepository
            .Setup(repository => repository.SearchProjects(It.IsAny<ProjectSearchSettings>()))
            .Returns([]);

        // Execution
        _searchService.SearchProjects(settings);

        // Asserts
        _searchRepository.Verify(repository => repository.SearchProjects(
            It.Is<ProjectSearchSettings>(searchSettings =>
                searchSettings.ProjectName == "Holiday" &&
                searchSettings.ProjectPath == @"C:\Photos\Holiday"
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
        _searchRepository
            .Setup(repository => repository.CountProjects(It.IsAny<ProjectSearchSettings>()))
            .Returns(0);

        _searchRepository
            .Setup(repository => repository.SearchProjects(It.IsAny<ProjectSearchSettings>()))
            .Returns([]);

        // Execution
        _searchService.SearchProjects(settings);

        // Asserts
        _searchRepository.Verify(repository => repository.SearchProjects(
            It.Is<ProjectSearchSettings>(searchSettings =>
                searchSettings.ProjectName == null &&
                searchSettings.ProjectPath == null
            )
        ), Times.Once);
    }
}