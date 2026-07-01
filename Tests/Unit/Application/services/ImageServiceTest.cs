using Application.interfaces.infrastructure;
using Application.interfaces.infrastructure.repositories;
using Application.services;
using Domain.entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Unit.Application.services;

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
}