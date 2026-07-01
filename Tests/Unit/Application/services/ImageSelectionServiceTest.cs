using Application.interfaces.infrastructure.repositories;
using Application.interfaces.services;
using Application.services;
using Domain.entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Unit.Application.services;

[TestFixture]
public class ImageSelectionServiceTests
{
    private Mock<ILogger<IImageSelectionService>> _logger = null!;
    private Mock<ISelectionRepository> _selectionRepository = null!;
    private Mock<IProjectRepository> _projectRepository = null!;
    private ImageSelectionService _imageSelectionService = null!;

    [SetUp]
    public void SetUp()
    {
        _logger = new Mock<ILogger<IImageSelectionService>>();
        _selectionRepository = new Mock<ISelectionRepository>();
        _projectRepository = new Mock<IProjectRepository>();

        _imageSelectionService = new ImageSelectionService(
            _logger.Object,
            _selectionRepository.Object,
            _projectRepository.Object
        );
    }

    [Test]
    public void StartSession_WithProject_StartsSession()
    {
        Project project = new Project(
            "Test project",
            @"C:\Photography\TestProject",
            DateOnly.FromDateTime(DateTime.Today),
            0,
            5
        );
        SelectionSession expectedSession = new SelectionSession(5, "test project", [], 0);

        // Mocks
        _selectionRepository
            .Setup(repository => repository.StartSession(5, "Test project"))
            .Returns(expectedSession);

        // Execution
        SelectionSession result = _imageSelectionService.StartSession(project);

        // Asserts
        Assert.That(result, Is.EqualTo(expectedSession));

        _selectionRepository.Verify(
            repository => repository.StartSession(5, "Test project"),
            Times.Once
        );
    }

    [Test]
    public void StartSession_WithProjectIdAndSessionName_StartsSession()
    {
        SelectionSession expectedSession = new SelectionSession(5, "Selection 1", [], 0);

        // Mocks
        _selectionRepository
            .Setup(repository => repository.StartSession(5, "Selection 1"))
            .Returns(expectedSession);

        // Execution
        SelectionSession result = _imageSelectionService.StartSession(5, "Selection 1");

        // Asserts
        Assert.That(result, Is.EqualTo(expectedSession));

        _selectionRepository.Verify(
            repository => repository.StartSession(5, "Selection 1"),
            Times.Once
        );
    }

    [Test]
    public void StartSession_WithoutSessionName_UsesProjectName()
    {
        Project project = new Project(
            "Project name from repository",
            @"C:\Photography\TestProject",
            DateOnly.FromDateTime(DateTime.Today),
            0,
            5
        );

        SelectionSession expectedSession = new SelectionSession(5, "Project name from repository", [], 0);

        // Mocks
        _projectRepository
            .Setup(repository => repository.GetById(5))
            .Returns(project);

        _selectionRepository
            .Setup(repository => repository.StartSession(5, "Project name from repository"))
            .Returns(expectedSession);

        // Execution
        SelectionSession result = _imageSelectionService.StartSession(5, null);

        // Asserts
        Assert.That(result, Is.EqualTo(expectedSession));

        _projectRepository.Verify(
            repository => repository.GetById(5),
            Times.Once
        );

        _selectionRepository.Verify(
            repository => repository.StartSession(5, "Project name from repository"),
            Times.Once
        );
    }

    [Test]
    public void GetOrStartSession_WithProject_GetsOrStartsSession()
    {
        Project project = new Project(
            "Test project",
            @"C:\Photography\TestProject",
            DateOnly.FromDateTime(DateTime.Today),
            null,
            5
        );

        SelectionSession expectedSession = new SelectionSession( 5, "Test project", [], 0);

        // Mocks
        _selectionRepository
            .Setup(repository => repository.GetOrStartSession(5, "Test project"))
            .Returns(expectedSession);

        // Execution
        SelectionSession result = _imageSelectionService.GetOrStartSession(project);

        // Asserts
        Assert.That(result, Is.EqualTo(expectedSession));

        _selectionRepository.Verify(
            repository => repository.GetOrStartSession(5, "Test project"),
            Times.Once
        );
    }

    [Test]
    public void GetOrStartSession_WithProjectIdAndSessionName_GetsOrStartsSession()
    {
        SelectionSession expectedSession = new SelectionSession(5, "Selection 1", [], 0);

        // Mocks
        _selectionRepository
            .Setup(repository => repository.GetOrStartSession(5, "Selection 1"))
            .Returns(expectedSession);

        // Execution
        SelectionSession result = _imageSelectionService.GetOrStartSession(5, "Selection 1");

        // Asserts
        Assert.That(result, Is.EqualTo(expectedSession));

        _selectionRepository.Verify(
            repository => repository.GetOrStartSession(5, "Selection 1"),
            Times.Once
        );
    }

    [Test]
    public void GetOrStartSession_WithoutSessionName_UsesProjectName()
    {
        Project project = new Project(
            "Project name from repository",
            @"C:\Photography\TestProject",
            DateOnly.FromDateTime(DateTime.Today),
            null,
            5
        );

        SelectionSession expectedSession = new SelectionSession(5, "Project name from repository", [], 0);

        // Mocks
        _projectRepository
            .Setup(repository => repository.GetById(5))
            .Returns(project);

        _selectionRepository
            .Setup(repository => repository.GetOrStartSession(5, "Project name from repository"))
            .Returns(expectedSession);

        // Execution
        SelectionSession result = _imageSelectionService.GetOrStartSession(5, null);

        // Asserts
        Assert.That(result, Is.EqualTo(expectedSession));

        _projectRepository.Verify(
            repository => repository.GetById(5),
            Times.Once
        );

        _selectionRepository.Verify(
            repository => repository.GetOrStartSession(5, "Project name from repository"),
            Times.Once
        );
    }

    [Test]
    public void GetSessionId_ReturnsSessionId()
    {
        // Mocks
        _selectionRepository
            .Setup(repository => repository.GetSessionIdByProjectId(5))
            .Returns(12);

        // Execution
        int? result = _imageSelectionService.GetSessionId(5);

        // Asserts
        Assert.That(result, Is.EqualTo(12));

        _selectionRepository.Verify(
            repository => repository.GetSessionIdByProjectId(5),
            Times.Once
        );
    }

    [Test]
    public void RemoveSession_WithProject_RemovesSession()
    {
        Project project = new Project(
            "Test project",
            @"C:\Photography\TestProject",
            DateOnly.FromDateTime(DateTime.Today),
            0,
            5
        );

        // Execution
        _imageSelectionService.RemoveSession(project);

        // Asserts
        _selectionRepository.Verify(
            repository => repository.RemoveSession(5),
            Times.Once
        );
    }

    [Test]
    public void RemoveSession_WithProjectId_RemovesSession()
    {
        // Execution
        _imageSelectionService.RemoveSession(5);

        // Asserts
        _selectionRepository.Verify(
            repository => repository.RemoveSession(5),
            Times.Once
        );
    }

    [Test]
    public void ClearSession_WithProject_ClearsSession()
    {
        Project project = new Project(
            "Test project",
            @"C:\Photography\TestProject",
            DateOnly.FromDateTime(DateTime.Today),
            null,
            5
        );

        // Execution
        _imageSelectionService.ClearSession(project);

        // Asserts
        _selectionRepository.Verify(
            repository => repository.ClearSession(5),
            Times.Once
        );
    }

    [Test]
    public void ClearSession_WithProjectId_ClearsSession()
    {
        // Execution
        _imageSelectionService.ClearSession(5);

        // Asserts
        _selectionRepository.Verify(
            repository => repository.ClearSession(5),
            Times.Once
        );
    }

    [Test]
    public void SelectImage_AddsImageToSelection()
    {
        Image image = new Image(
            5,
            "DSC_1234",
            ".NEF",
            @"Original\DSC_1234.NEF",
            10
        );

        // Execution
        _imageSelectionService.SelectImage(image);

        // Asserts
        _selectionRepository.Verify(
            repository => repository.AddImageToProjectSelection(5, 10),
            Times.Once
        );
    }

    [Test]
    public void AddImageToSelection_AddsImageToSelection()
    {
        // Execution
        _imageSelectionService.AddImageToSelection(12, 10);

        // Asserts
        _selectionRepository.Verify(
            repository => repository.AddImageToProjectSelection(12, 10),
            Times.Once
        );
    }

    [Test]
    public void RemoveImageFromSelection_RemovesImageFromSelection()
    {
        // Execution
        _imageSelectionService.RemoveImageFromSelection(12, 10);

        // Asserts
        _selectionRepository.Verify(
            repository => repository.RemoveImageFromProjectSelection(12, 10),
            Times.Once
        );
    }

    [Test]
    public void GetSessionImages_WithProject_ReturnsSession()
    {
        Project project = new Project(
            "Test project",
            @"C:\Photography\TestProject",
            DateOnly.FromDateTime(DateTime.Today),
            null,
            5
        );

        SelectionSession expectedSession = new SelectionSession(5, "Test project", [1, 2, 3], 0);

        // Mocks
        _selectionRepository
            .Setup(repository => repository.GetByProject(5))
            .Returns(expectedSession);

        // Execution
        SelectionSession result = _imageSelectionService.GetSessionImages(project);

        // Asserts
        Assert.That(result, Is.EqualTo(expectedSession));

        _selectionRepository.Verify(
            repository => repository.GetByProject(5),
            Times.Once
        );
    }

    [Test]
    public void GetSessionImages_WithProjectId_ReturnsSession()
    {
        SelectionSession expectedSession = new SelectionSession(5, "Test project", [1, 2, 3], 0);

        // Mocks
        _selectionRepository
            .Setup(repository => repository.GetByProject(5))
            .Returns(expectedSession);

        // Execution
        SelectionSession result = _imageSelectionService.GetSessionImages(5);

        // Asserts
        Assert.That(result, Is.EqualTo(expectedSession));

        _selectionRepository.Verify(
            repository => repository.GetByProject(5),
            Times.Once
        );
    }

    [Test]
    public void ImageIsSelected_ReturnsTrue_WhenImageIsSelected()
    {
        // Mocks
        _selectionRepository
            .Setup(repository => repository.ImageIsSelected(12, 10))
            .Returns(true);

        // Execution
        bool result = _imageSelectionService.ImageIsSelected(12, 10);

        // Asserts
        Assert.That(result, Is.True);

        _selectionRepository.Verify(
            repository => repository.ImageIsSelected(12, 10),
            Times.Once
        );
    }

    [Test]
    public void ImageIsSelected_ReturnsFalse_WhenImageIsNotSelected()
    {
        // Mocks
        _selectionRepository
            .Setup(repository => repository.ImageIsSelected(12, 10))
            .Returns(false);

        // Execution
        bool result = _imageSelectionService.ImageIsSelected(12, 10);

        // Asserts
        Assert.That(result, Is.False);

        _selectionRepository.Verify(
            repository => repository.ImageIsSelected(12, 10),
            Times.Once
        );
    }

    [Test]
    public void ToggleImageSelection_RemovesImageAndReturnsFalse_WhenImageIsAlreadySelected()
    {
        // Mocks
        _selectionRepository
            .Setup(repository => repository.ImageIsSelected(12, 10))
            .Returns(true);

        // Execution
        bool result = _imageSelectionService.ToggleImageSelection(12, 10);

        // Asserts
        Assert.That(result, Is.False);

        _selectionRepository.Verify(
            repository => repository.RemoveImageFromProjectSelection(12, 10),
            Times.Once
        );

        _selectionRepository.Verify(
            repository => repository.AddImageToProjectSelection(
                It.IsAny<int>(),
                It.IsAny<int>()
            ),
            Times.Never
        );
    }

    [Test]
    public void ToggleImageSelection_AddsImageAndReturnsTrue_WhenImageIsNotSelected()
    {
        // Mocks
        _selectionRepository
            .Setup(repository => repository.ImageIsSelected(12, 10))
            .Returns(false);

        // Execution
        bool result = _imageSelectionService.ToggleImageSelection(12, 10);

        // Asserts
        Assert.That(result, Is.True);

        _selectionRepository.Verify(
            repository => repository.AddImageToProjectSelection(12, 10),
            Times.Once
        );

        _selectionRepository.Verify(
            repository => repository.RemoveImageFromProjectSelection(
                It.IsAny<int>(),
                It.IsAny<int>()
            ),
            Times.Never
        );
    }
}