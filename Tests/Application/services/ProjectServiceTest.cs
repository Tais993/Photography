using Application;
using Application.interfaces;
using Application.services;
using Application.services.interfaces;
using Domain.entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Application.services;

[TestFixture]
[TestOf(typeof(ProjectService))]
public class ProjectServiceTest
{
    private Mock<IProjectRepository> _projectRepository = null!;
    private Mock<IImageRepository> _imageRepository = null!;
    private Mock<IFiles> _files = null!;
    private Mock<ILogger<ProjectService>> _logger = null!;
    private Mock<IProjectMetadataService> _metadataService = null!;
    private ProjectService _projectService = null!;

    [SetUp]
    public void SetUp()
    {
        _projectRepository = new Mock<IProjectRepository>();
        _imageRepository = new Mock<IImageRepository>();
        _files = new Mock<IFiles>();
        _logger = new Mock<ILogger<ProjectService>>();
        _metadataService = new Mock<IProjectMetadataService>();

        _projectService = new ProjectService(
            _projectRepository.Object,
            _imageRepository.Object,
            _files.Object,
            _logger.Object
        );
    }

    [Test]
    public void ResolveProject_ProjectExists_ReturnsProject_WithEmptyProjectId()
    {
        const string projectDirectory = @"C:\2024-07-04-Merijn";
        const string projectInfoPath = @"C:\2024-07-04-Merijn\project.info";

        Project expectedProject = new Project(
            2,
            "Merijn",
            projectDirectory,
            new DateOnly(2024, 7, 4)
        );

        // Mocks
        _files
            .Setup(f => f.Combine(projectDirectory, Constants.ProjectInfoFile))
            .Returns(projectInfoPath);

        _files
            .Setup(f => f.Exists(projectInfoPath))
            .Returns(true);

        _files
            .Setup(f => f.ReadAllText(projectInfoPath))
            .Returns("2");

        _projectRepository
            .Setup(r => r.GetById(2))
            .Returns(expectedProject);

        // Execution
        Project? result = _projectService.ResolveProject(projectDirectory, 0);

        
        // Asserts
        Assert.That(result, Is.EqualTo(expectedProject));
        _projectRepository.Verify(r => r.GetById(2), Times.Once);
    }
    
    [Test]
    public void ResolveProject_ProjectExists_ReturnsProject_WithProjectId()
    {
        const string projectDirectory = @"C:\2024-07-04-Merijn";

        Project expectedProject = new Project(
            2,
            "Merijn",
            projectDirectory,
            new DateOnly(2024, 7, 4)
        );

        // Mocks
        _projectRepository
            .Setup(r => r.GetById(2))
            .Returns(expectedProject);

        // Execution
        Project? result = _projectService.ResolveProject(projectDirectory, 2);

        
        // Asserts
        Assert.That(result, Is.EqualTo(expectedProject));
        _projectRepository.Verify(r => r.GetById(2), Times.Once);
    }
    
    [Test]
    public void ResolveProjectId_ProjectExists_ReturnsProjectId_WithEmptyProjectId()
    {
        const string projectDirectory = @"C:\2024-07-04-Merijn";
        const string projectInfoPath = @"C:\2024-07-04-Merijn\project.info";
        
        const int projectId = 6;

        // Mocks
        _files
            .Setup(f => f.Combine(projectDirectory, Constants.ProjectInfoFile))
            .Returns(projectInfoPath);

        _files
            .Setup(f => f.Exists(projectInfoPath))
            .Returns(true);

        _files
            .Setup(f => f.ReadAllText(projectInfoPath))
            .Returns(projectId + "");
        
        // Execution
        int result = _projectService.ResolveProjectId(projectDirectory, 0);

        // Asserts
        Assert.That(result, Is.EqualTo(projectId));
        _files.Verify(f => f.ReadAllText(It.IsAny<string>()), Times.Once);
        _projectRepository.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }
    
    [Test]
    public void ResolveProjectId_ProjectNotInitialised_ReturnsZero_WithProjectId()
    {
        const string projectDirectory = @"C:\2024-07-04-Merijn";
        const string projectInfoPath = @"C:\2024-07-04-Merijn\project.info";
        
        const int projectId = 6;

        // Mocks
        // Execution
        int result = _projectService.ResolveProjectId(projectDirectory, projectId);

        // Asserts
        Assert.That(result, Is.EqualTo(projectId));
        _files.Verify(f => f.ReadAllText(It.IsAny<string>()), Times.Never);
        _projectRepository.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }
    
    
    [Test]
    public void ResolveProjectId_ProjectNotInitialised_ReturnsZero()
    {
        const string projectDirectory = @"C:\2024-07-04-Merijn";
        const string projectInfoPath = @"C:\2024-07-04-Merijn\project.info";

        // Mocks
        _files
            .Setup(f => f.Combine(projectDirectory, Constants.ProjectInfoFile))
            .Returns(projectInfoPath);

        _files
            .Setup(f => f.Exists(projectInfoPath))
            .Returns(false);

        // Execution
        int result = _projectService.ResolveProjectId(projectDirectory);

        // Asserts
        Assert.That(result, Is.EqualTo(0));
        _files.Verify(f => f.ReadAllText(It.IsAny<string>()), Times.Never);
        _projectRepository.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
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
        Image result = _projectService.GetImageById(5);

        // Asserts
        Assert.That(result, Is.EqualTo(expectedImage));
    }

    [Test]
    public void GetProjectImageCount_ReturnsCount()
    {
        // Mocks
        _imageRepository
            .Setup(r => r.GetProjectImageCount(2))
            .Returns(42);

        // Execution
        int result = _projectService.GetProjectImageCount(2);

        // Asserts
        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void GetProjectCount_ReturnsCount()
    {
        // Mocks
        _projectRepository
            .Setup(r => r.GetProjectCount())
            .Returns(7);

        // Execution
        int result = _projectService.GetProjectCount();

        // Asserts
        Assert.That(result, Is.EqualTo(7));
    }

    [Test]
    public void GetProjectById_ReturnsProject()
    {
        Project expectedProject = new Project(
            2,
            "Merijn",
            @"C:\2024-07-04-Merijn",
            new DateOnly(2024, 7, 4)
        );

        // Mocks
        _projectRepository
            .Setup(r => r.GetById(2))
            .Returns(expectedProject);

        // Execution
        Project result = _projectService.GetProjectById(2);

        // Asserts
        Assert.That(result, Is.EqualTo(expectedProject));
    }

    [Test]
    public void GetAllProjects_ReturnsProjects()
    {
        List<Project> expectedProjects = new List<Project>
        {
            new Project(1, "Ardennen", @"C:\2026-06-01-Ardennen", new DateOnly(2026, 6, 1)),
            new Project(2, "Antwerpen", @"C:\2024-01-10-Antwerpen", new DateOnly(2026, 1, 10))
        };

        // Mocks
        _projectRepository
            .Setup(r => r.GetAll())
            .Returns(expectedProjects);

        // Execution
        List<Project> result = _projectService.GetAllProjects();

        // Asserts
        Assert.That(result, Is.EqualTo(expectedProjects));
    }
}