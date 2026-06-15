using Application.interfaces;
using Application.services.imageviewers;
using Moq;

namespace Tests.services.imageviewers;

[TestFixture]
[TestOf(typeof(IrfanViewService))]
public class IrfanViewServiceTest
{
    private Mock<IIrfanViewGateway> _irfanViewGateway = null!;
    private Mock<IFiles> _files = null!;
    private IrfanViewService _irfanViewService = null!;

    [SetUp]
    public void SetUp()
    {
        _irfanViewGateway = new Mock<IIrfanViewGateway>();
        _files = new Mock<IFiles>();

        _irfanViewService = new IrfanViewService(
            _irfanViewGateway.Object,
            _files.Object
        );
    }

    [Test]
    public void IsAvailable_ReturnsTrue()
    {
        // Execution
        bool result = _irfanViewService.IsAvailable();

        // Asserts
        Assert.That(result, Is.True);
    }

    [Test]
    public void GetImageViewerName_ReturnsIrfanView()
    {
        // Execution
        string result = _irfanViewService.GetImageViewerName();

        // Asserts
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void GetOpenedFileName_ReturnsGivenFileName_WhenGivenFileNameIsNotNull()
    {
        // Execution
        string? result = _irfanViewService.GetOpenedFileName("DSC_1234");

        // Asserts
        Assert.That(result, Is.EqualTo("DSC_1234"));

        _irfanViewGateway.Verify(g => g.IsOpen(), Times.Never);
        _irfanViewGateway.Verify(g => g.GetOpenedFile(), Times.Never);
    }

    [Test]
    public void GetOpenedFileName_ReturnsOpenedFileName_WhenIrfanViewIsOpen()
    {
        // Mocks
        _irfanViewGateway
            .Setup(g => g.IsOpen())
            .Returns(true);

        _irfanViewGateway
            .Setup(g => g.GetOpenedFile())
            .Returns(@"C:\Photos\DSC_1234.JPG");

        _files
            .Setup(f => f.GetFileNameWithoutExtension(@"C:\Photos\DSC_1234.JPG"))
            .Returns("DSC_1234");

        // Execution
        string? result = _irfanViewService.GetOpenedFileName(null);

        // Asserts
        Assert.That(result, Is.EqualTo("DSC_1234"));
    }

    [Test]
    public void GetOpenedFileName_ReturnsNull_WhenIrfanViewIsNotOpen()
    {
        // Mocks
        _irfanViewGateway
            .Setup(g => g.IsOpen())
            .Returns(false);

        // Execution
        string? result = _irfanViewService.GetOpenedFileName(null);

        // Asserts
        Assert.That(result, Is.Null);

        _irfanViewGateway.Verify(g => g.GetOpenedFile(), Times.Never);
    }

    [Test]
    public void OpenImage_OpensFileWithGateway()
    {
        string imagePath = @"C:\Photos\DSC_1234.JPG";

        // Execution
        _irfanViewService.OpenImage(imagePath);

        // Asserts
        _irfanViewGateway.Verify(g => g.OpenFile(imagePath), Times.Once);
    }
}