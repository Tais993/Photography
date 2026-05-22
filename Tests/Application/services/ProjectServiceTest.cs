using Application.services;
using Domain.entities;
using Infrastructure.database.repositories;
using Infrastructure.filesystem;
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
    private Mock<ILogger<ProjectService>> _projectLogger = null!;
    private ProjectService _projectService = null!;

    [SetUp]
    public void SetUp()
    {
        _projectRepository = new Mock<IProjectRepository>();
        _imageRepository = new Mock<IImageRepository>();
        _files = new Mock<IFiles>();
        _projectLogger = new Mock<ILogger<ProjectService>>();

        _projectService = new ProjectService(_projectRepository.Object, _imageRepository.Object, _files.Object,
            _projectLogger.Object);
    }

    [Test]
    public void ResolveProject_ProjectExists()
    {
        const string projectDirectory = "C:\\2024-07-4-Merijn";
        const string projectInfoDirectory = "C:\\2024-07-4-Merijn";

        Project expectedProject = new Project(
            2,
            "Merijn",
            projectDirectory,
            new DateOnly(2024, 7, 4)
        );

        // Mocks
        _files.Setup(f => f.Combine(It.IsAny<string[]>())).Returns(projectInfoDirectory);
        _files.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns("2");
        _files.Setup(f => f.Exists(It.IsAny<string>())).Returns(true);

        _projectRepository.Setup(r => r.GetById(2))
            .Returns(expectedProject);

        // Execution
        var result = _projectService.ResolveProject(projectDirectory);

        // Asserts
        Assert.That(result, Is.EqualTo(expectedProject));
    }

    [Test]
    public void ResolveProject_ProjectNotInitialized()
    {
        const string projectDirectory = "C:\\2024-07-4-Merijn";
        const string projectInfoDirectory = "C:\\2024-07-4-Merijn";

        // Mocks
        _files.Setup(f => f.Combine(It.IsAny<string>())).Returns(projectInfoDirectory);
        _files.Setup(f => f.Exists(It.IsAny<string>())).Returns(false);

        // Execution
        int id = _projectService.ResolveProjectId(projectDirectory);

        // Asserts
        Assert.That(id, Is.EqualTo(0));
    }


    [Test]
    public void InitialiseProject_IgnoresCategoryFolders()
    {
        const string projectDirectory = "C:\\.2024-07-4-Merijn";

        // Mocks
        _files.Setup(f => f.GetPathEnd(It.IsAny<string>())).Returns(".2024-07-04-Merijn");

        // Execution
        _projectService.InitialiseExistingFolder(projectDirectory);

        // Asserts
        _files.Verify(f => f.Combine(It.IsAny<string[]>()), Times.Never);
        _files.Verify(f => f.Exists(It.IsAny<string>()), Times.Never);
        _files.Verify(f => f.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _projectRepository.Verify(r => r.Insert(It.IsAny<Project>()), Times.Never);
    }


    [Test]
    public void InitialiseProject_IgnoresIncorrectFolderNames()
    {
        const string projectDirectory = "C:\\2024-0a74-Merijn";

        // Mocks
        _files.Setup(f => f.GetPathEnd(It.IsAny<string>())).Returns("2024-0a74-Merijn");

        // Execution
        _projectService.InitialiseExistingFolder(projectDirectory);

        // Asserts
        _files.Verify(f => f.Combine(It.IsAny<string[]>()), Times.Never);
        _files.Verify(f => f.Exists(It.IsAny<string>()), Times.Never);
        _files.Verify(f => f.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _projectRepository.Verify(r => r.Insert(It.IsAny<Project>()), Times.Never);
    }


    [Test]
    public void InitialiseProject_IgnoresAlreadyInitialisedProject()
    {
        const string projectDirectory = @"C:\2024-07-04-Merijn";
        const string projectInfoDirectory = @"C:\2024-07-04-Merijn\project.info";

        // Mocks
        _files.Setup(f => f.Combine(It.IsAny<string[]>())).Returns(projectInfoDirectory);
        _files.Setup(f => f.GetPathEnd(It.IsAny<string>())).Returns("2024-07-04-Merijn");
        _files.Setup(f => f.Exists(It.IsAny<string>())).Returns(true);

        // Execution
        _projectService.InitialiseExistingFolder(projectDirectory);

        // Asserts
        _files.Verify(f => f.Combine(It.IsAny<string[]>()), Times.Once);
        _files.Verify(f => f.Exists(It.IsAny<string>()), Times.Once);
        _files.Verify(f => f.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _projectRepository.Verify(r => r.Insert(It.IsAny<Project>()), Times.Never);
    }


    [Test]
    public void InitialiseProject_SuccessfullySavesData()
    {
        const string projectDirectory = @"C:\2024-07-04-Merijn";
        const string projectInfoDirectory = @"C:\2024-07-04-Merijn\project.info";

        const int projectId = 2;


        Project expectedProject = new Project(
            projectId,
            "Merijn",
            projectDirectory,
            new DateOnly(2024, 7, 4)
        );

        // Mocks
        _files.Setup(f => f.Combine(It.IsAny<string[]>())).Returns(projectInfoDirectory);
        _files.Setup(f => f.GetPathEnd(It.IsAny<string>())).Returns("2024-07-04-Merijn");
        _files.Setup(f => f.Exists(It.IsAny<string>())).Returns(false);
        _files.Setup(f =>
            f.WriteAllText(It.Is<string>(s => s == 2 + ""), It.Is<string>(s => s == projectInfoDirectory)));

        _projectRepository.Setup(r => r.Insert(It.IsAny<Project>())).Returns((Project project) => new Project(
            projectId,
            project.Name,
            project.Path,
            project.EventDate
        ));


        // Execution
        _projectService.InitialiseExistingFolder(projectDirectory);

        // Assert
        _files.Verify(f => f.Combine(projectDirectory, ProjectService.ProjectInfoFile), Times.Once);
        _files.Verify(f => f.Exists(projectInfoDirectory), Times.Once);

        _projectRepository.Verify(r => r.Insert(It.Is<Project>(p =>
            p.Id == null &&
            p.Name == "Merijn" &&
            p.Path == projectDirectory &&
            p.EventDate == new DateOnly(2024, 7, 4)
        )), Times.Once);

        _files.Verify(f => f.WriteAllText("2", projectInfoDirectory), Times.Once);
    }


    [Test]
    public void InitialiseProject_SuccessfullySavesData_SingleDigitMonthAndDay()
    {
        const string projectDirectory = @"C:\2024-7-4-Merijn";
        const string projectInfoDirectory = @"C:\2024-7-4-Merijn\project.info";

        const int projectId = 2;


        Project expectedProject = new Project(
            projectId,
            "Merijn",
            projectDirectory,
            new DateOnly(2024, 7, 4)
        );

        // Mocks
        _files.Setup(f => f.Combine(It.IsAny<string[]>())).Returns(projectInfoDirectory);
        _files.Setup(f => f.GetPathEnd(It.IsAny<string>())).Returns("2024-7-4-Merijn");
        _files.Setup(f => f.Exists(It.IsAny<string>())).Returns(false);
        _files.Setup(f =>
            f.WriteAllText(It.Is<string>(s => s == 2 + ""), It.Is<string>(s => s == projectInfoDirectory)));

        _projectRepository.Setup(r => r.Insert(It.IsAny<Project>())).Returns((Project project) => new Project(
            projectId,
            project.Name,
            project.Path,
            project.EventDate
        ));


        // Execution
        _projectService.InitialiseExistingFolder(projectDirectory);

        // Assert
        _files.Verify(f => f.Combine(projectDirectory, ProjectService.ProjectInfoFile), Times.Once);
        _files.Verify(f => f.Exists(projectInfoDirectory), Times.Once);

        _projectRepository.Verify(r => r.Insert(It.Is<Project>(p =>
            p.Id == null &&
            p.Name == "Merijn" &&
            p.Path == projectDirectory &&
            p.EventDate == new DateOnly(2024, 7, 4)
        )), Times.Once);

        _files.Verify(f => f.WriteAllText("2", projectInfoDirectory), Times.Once);
    }

    [Test]
    public void InitialiseProject_SuccessfullySavesData_SpacesInProjectName()
    {
        const string projectDirectory = @"C:\2024-07-04-Merijn gymnasium";
        const string projectInfoDirectory = @"C:\2024-07-04-Merijn gymnasium\project.info";

        const int projectId = 2;


        Project expectedProject = new Project(
            projectId,
            "Merijn gymnasium",
            projectDirectory,
            new DateOnly(2024, 7, 4)
        );

        // Mocks
        _files.Setup(f => f.Combine(It.IsAny<string[]>())).Returns(projectInfoDirectory);
        _files.Setup(f => f.GetPathEnd(It.IsAny<string>())).Returns("2024-07-04-Merijn gymnasium");
        _files.Setup(f => f.Exists(It.IsAny<string>())).Returns(false);
        _files.Setup(f =>
            f.WriteAllText(It.Is<string>(s => s == 2 + ""), It.Is<string>(s => s == projectInfoDirectory)));

        _projectRepository.Setup(r => r.Insert(It.IsAny<Project>())).Returns((Project project) => new Project(
            projectId,
            project.Name,
            project.Path,
            project.EventDate
        ));


        // Execution
        _projectService.InitialiseExistingFolder(projectDirectory);

        // Assert
        _files.Verify(f => f.Combine(projectDirectory, ProjectService.ProjectInfoFile), Times.Once);
        _files.Verify(f => f.Exists(projectInfoDirectory), Times.Once);

        _projectRepository.Verify(r => r.Insert(It.Is<Project>(p =>
            p.Id == null &&
            p.Name == "Merijn gymnasium" &&
            p.Path == projectDirectory &&
            p.EventDate == new DateOnly(2024, 7, 4)
        )), Times.Once);

        _files.Verify(f => f.WriteAllText("2", projectInfoDirectory), Times.Once);
    }


    [Test]
    public void ProjectNameRegex_MatchesValidProjectFolder()
    {
        // Arrange
        const string folderName = "2024-07-04-Merijn";

        // Act
        var match = ProjectService.ProjectNameRegex.Match(folderName);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(match.Success, Is.True);
            Assert.That(match.Groups[1].Value, Is.EqualTo("2024"));
            Assert.That(match.Groups[2].Value, Is.EqualTo("07"));
            Assert.That(match.Groups[3].Value, Is.EqualTo("04"));
            Assert.That(match.Groups[4].Value, Is.EqualTo("Merijn"));
        });
    }

    [Test]
    public void ProjectNameRegex_DoesNotMatchInvalidProjectFolder()
    {
        // Arrange
        const string folderName = "invalid-folder-name";

        // Act
        var match = ProjectService.ProjectNameRegex.Match(folderName);

        // Assert
        Assert.That(match.Success, Is.False);
    }
}