using Application.interfaces;
using Application.services;
using Domain.entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.services;

[TestFixture]
[TestOf(typeof(ImageService))]
public class ImageServiceTest
{
    private Mock<IImageRepository> _imageRepository = null!;
    private Mock<ILogger<ImageService>> _logger = null!;
    private ImageService _imageService = null!;

    [SetUp]
    public void SetUp()
    {
        _imageRepository = new Mock<IImageRepository>();
        _logger = new Mock<ILogger<ImageService>>();

        _imageService = new ImageService(
            _imageRepository.Object,
            _logger.Object
        );
    }

    [Test]
    public void GetImageById_ReturnsImage()
    {
        Image expectedImage = new Image(
            1,
            "DSC_1234",
            ".NEF",
            @"Original\DSC_1234.NEF"
        );

        // Mocks
        _imageRepository
            .Setup(r => r.GetById(5))
            .Returns(expectedImage);

        // Execution
        Image result = _imageService.GetImageById(5);

        // Asserts
        Assert.That(result, Is.EqualTo(expectedImage));
    }

    
    [Test]
    public void GetImagesByProjectId_ReturnsImages()
    {
        List<Image> expectedImages =
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
            .Setup(r => r.GetAllByProjectId(2))
            .Returns(expectedImages);

        // Execution
        List<Image> result = _imageService.GetImagesByProjectId(2);

        // Asserts
        Assert.That(result, Is.EqualTo(expectedImages));
    }
    
    
    [Test]
    public void GetProjectImageCount_ReturnsCount()
    {
        // Mocks
        _imageRepository
            .Setup(r => r.GetProjectImageCount(2))
            .Returns(42);

        // Execution
        int result = _imageService.GetProjectImageCount(2);

        // Asserts
        Assert.That(result, Is.EqualTo(42));
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
        List<Image> result = _imageService
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
        List<Image> result = _imageService
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
        List<Image> result = _imageService
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
        List<Image> result = _imageService
            .HideRawFilesWhenNonRawExists(images)
            .ToList();

        // Asserts
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].FileName, Is.EqualTo("dsc_1234.jpg"));
    }
}