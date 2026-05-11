using Application.services;
using Application.services.interfaces;
using Infrastructure.database.repositories;
using Moq;

namespace Tests.Application.services.interfaces;

[TestFixture]
[TestOf(typeof(IFileSearchService))]
public class IFileSearchServiceTest
{
    private Mock<IImageRepository> _imageRepository = null!;
    private FileSearchService _fileSearchService = null!;

    [SetUp]
    public void SetUp()
    {
        _imageRepository = new Mock<IImageRepository>();

        _fileSearchService = new FileSearchService(_imageRepository.Object);
    }
    
    [Test]
    public void searchImagesByNameOrNumber_CorrectlyIdentifyName()
    {
        // Mocks
        _imageRepository.Setup(repo => repo.GetImagesByFileName(It.IsAny<string>())
        ).Returns([]);

        // Executions
        _fileSearchService.searchImagesByNameOrNumber("DSC_0350");
        
        // Asserts
        _imageRepository.Verify(
            repo => repo.GetImagesByFileName(It.IsAny<string>()), Times.Once);
    }
    
    [Test]
    public void searchImagesByNameOrNumber_CorrectlyIdentifyNumber()
    {
        // Mocks
        _imageRepository.Setup(repo => repo.GetImagesByPhotoNumber(It.IsAny<string>())
            ).Returns([]);
        
        // Executions
        _fileSearchService.searchImagesByNameOrNumber("0350");
        
        // Asserts
        _imageRepository.Verify(
            repo => repo.GetImagesByPhotoNumber(It.IsAny<string>()), Times.Once);
    }
}