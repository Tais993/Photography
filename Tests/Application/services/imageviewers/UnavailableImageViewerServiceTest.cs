using Application.services.imageviewers;

namespace Tests.services.imageviewers;

[TestFixture]
[TestOf(typeof(UnavailableImageViewerService))]
public class UnavailableImageViewerServiceTest
{
    
    [Test]
    public void IsAvailable_ReturnsFalse()
    {
        UnavailableImageViewerService service = new("Image viewer is not configured");

        // Execution
        bool result = service.IsAvailable();

        // Asserts
        Assert.That(result, Is.False);
    }

    [Test]
    public void GetImageViewerName_ReturnsUnavailable()
    {
        UnavailableImageViewerService service = new("Image viewer is not configured");

        // Execution
        string result = service.GetImageViewerName();

        // Asserts
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void OpenImage_ThrowsInvalidOperationException()
    {
        UnavailableImageViewerService service = new("Image viewer is not configured");

        // Execution & Asserts
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            service.OpenImage(@"C:\Photos\DSC_1234.JPG")
        )!;

        Assert.That(exception.Message, Is.EqualTo("Image viewer is not configured"));
    }

    [Test]
    public void GetOpenedFileName_ReturnsGivenFileName()
    {
        UnavailableImageViewerService service = new("Image viewer is not configured");

        // Execution
        string? result = service.GetOpenedFileName("DSC_1234");

        // Asserts
        Assert.That(result, Is.EqualTo("DSC_1234"));
    }

    [Test]
    public void GetOpenedFileName_ThrowsInvalidOperationException_WhenGivenFileNameIsNull()
    {
        UnavailableImageViewerService service = new("Image viewer is not configured");

        // Execution & Asserts
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            service.GetOpenedFileName(null)
        )!;

        Assert.That(exception.Message, Is.EqualTo("Image viewer is not configured"));
    }
}

