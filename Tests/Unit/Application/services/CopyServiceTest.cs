using Application.interfaces.infrastructure;
using Application.interfaces.infrastructure.repositories;
using Application.services;
using Domain.entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Unit.Application.services;

[TestFixture]
[TestOf(typeof(CopyService))]
public class CopyServiceTest
{
    private Mock<IImageRepository> _imageRepository = null!;
    private Mock<IFiles> _files = null!;
    private Mock<ILogger<CopyService>> _logger = null!;
    private CopyService _copyService = null!;

    [SetUp]
    public void SetUp()
    {
        _imageRepository = new Mock<IImageRepository>();
        _files = new Mock<IFiles>();
        _logger = new Mock<ILogger<CopyService>>();

        _copyService = new CopyService(
            _logger.Object ,
            _files.Object,
            _imageRepository.Object
        );
    }

    [Test]
    public void ImageIdsToRelativePaths_ReturnsRelativePaths()
    {
        int[] imageIds = [1, 2];

        List<Image> images =
        [
            new Image(
                1,
                "DSC_1234",
                ".NEF",
                @"Original\DSC_1234.NEF"
            ),
            new Image(
                2,
                "DSC_1235",
                ".JPG",
                @"Original\DSC_1235.JPG"
            )
        ];

        // Mocks
        _imageRepository
            .Setup(repository => repository.GetAllByIds(imageIds))
            .Returns(images);

        // Execution
        List<string> result = _copyService
            .ImageIdsToRelativePaths(imageIds)
            .ToList();

        // Asserts
        Assert.That(result, Is.EqualTo(new List<string>
        {
            @"Original\DSC_1234.NEF",
            @"Original\DSC_1235.JPG"
        }));
    }

    [Test]
    public void ImageIdsToRelativePaths_CallsRepositoryWithImageIds()
    {
        int[] imageIds = [5, 10, 15];

        _imageRepository
            .Setup(repository => repository.GetAllByIds(imageIds))
            .Returns([]);

        // Execution
        _copyService.ImageIdsToRelativePaths(imageIds).ToList();

        // Asserts
        _imageRepository.Verify(
            repository => repository.GetAllByIds(imageIds),
            Times.Once
        );
    }

    [Test]
    public void CopyFiles_CopiesEveryRelativeFileToTargetDirectory()
    {
        IEnumerable<string> relativeFiles =
        [
            @"Original\DSC_1234.NEF",
            @"Original\DSC_1235.JPG"
        ];

        string projectPath = @"C:\Photography\2026-06-11-TestProject";
        string relativeTargetDirectory = "Editing";
        string targetDirectory = @"C:\Photography\2026-06-11-TestProject\Editing";

        // Mocks
        _files
            .Setup(files => files.Combine(projectPath, relativeTargetDirectory))
            .Returns(targetDirectory);

        _files
            .Setup(files => files.Combine(projectPath, @"Original\DSC_1234.NEF"))
            .Returns(@"C:\Photography\2026-06-11-TestProject\Original\DSC_1234.NEF");

        _files
            .Setup(files => files.Combine(projectPath, @"Original\DSC_1235.JPG"))
            .Returns(@"C:\Photography\2026-06-11-TestProject\Original\DSC_1235.JPG");

        _files
            .Setup(files => files.GetFileName(@"Original\DSC_1234.NEF"))
            .Returns("DSC_1234.NEF");

        _files
            .Setup(files => files.GetFileName(@"Original\DSC_1235.JPG"))
            .Returns("DSC_1235.JPG");

        _files
            .Setup(files => files.Combine(targetDirectory, "DSC_1234.NEF"))
            .Returns(@"C:\Photography\2026-06-11-TestProject\Editing\DSC_1234.NEF");

        _files
            .Setup(files => files.Combine(targetDirectory, "DSC_1235.JPG"))
            .Returns(@"C:\Photography\2026-06-11-TestProject\Editing\DSC_1235.JPG");

        // Execution
        _copyService.CopyFiles(relativeFiles, projectPath, relativeTargetDirectory);

        // Asserts
        _files.Verify(
            files => files.CopyFile(
                @"C:\Photography\2026-06-11-TestProject\Original\DSC_1234.NEF",
                @"C:\Photography\2026-06-11-TestProject\Editing\DSC_1234.NEF"
            ),
            Times.Once
        );

        _files.Verify(
            files => files.CopyFile(
                @"C:\Photography\2026-06-11-TestProject\Original\DSC_1235.JPG",
                @"C:\Photography\2026-06-11-TestProject\Editing\DSC_1235.JPG"
            ),
            Times.Once
        );

        _files.Verify(
            files => files.CopyFile(It.IsAny<string>(), It.IsAny<string>()),
            Times.Exactly(2)
        );
    }

    [Test]
    public void CopyFiles_DoesNotCopyFiles_WhenRelativeFilesIsEmpty()
    {
        IEnumerable<string> relativeFiles = [];

        string projectPath = @"C:\Photography\2026-06-11-TestProject";
        string relativeTargetDirectory = "Editing";
        string targetDirectory = @"C:\Photography\2026-06-11-TestProject\Editing";

        // Mocks
        _files
            .Setup(files => files.Combine(projectPath, relativeTargetDirectory))
            .Returns(targetDirectory);

        // Execution
        _copyService.CopyFiles(relativeFiles, projectPath, relativeTargetDirectory);

        // Asserts
        _files.Verify(
            files => files.CopyFile(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Test]
    public void CopyFiles_UsesFileNameOnlyForTargetFile()
    {
        IEnumerable<string> relativeFiles =
        [
            @"Original\Subfolder\DSC_1234.NEF"
        ];

        string projectPath = @"C:\Photography\2026-06-11-TestProject";
        string relativeTargetDirectory = "Editing";
        string targetDirectory = @"C:\Photography\2026-06-11-TestProject\Editing";

        // Mocks
        _files
            .Setup(files => files.Combine(projectPath, relativeTargetDirectory))
            .Returns(targetDirectory);

        _files
            .Setup(files => files.Combine(projectPath, @"Original\Subfolder\DSC_1234.NEF"))
            .Returns(@"C:\Photography\2026-06-11-TestProject\Original\Subfolder\DSC_1234.NEF");

        _files
            .Setup(files => files.GetFileName(@"Original\Subfolder\DSC_1234.NEF"))
            .Returns("DSC_1234.NEF");

        _files
            .Setup(files => files.Combine(targetDirectory, "DSC_1234.NEF"))
            .Returns(@"C:\Photography\2026-06-11-TestProject\Editing\DSC_1234.NEF");

        // Execution
        _copyService.CopyFiles(relativeFiles, projectPath, relativeTargetDirectory);

        // Asserts
        _files.Verify(
            files => files.CopyFile(
                @"C:\Photography\2026-06-11-TestProject\Original\Subfolder\DSC_1234.NEF",
                @"C:\Photography\2026-06-11-TestProject\Editing\DSC_1234.NEF"
            ),
            Times.Once
        );
    }
}