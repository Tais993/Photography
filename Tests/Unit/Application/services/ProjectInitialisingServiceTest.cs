using Application;
using Application.interfaces.infrastructure;
using Application.interfaces.services;
using Application.services;
using Domain.entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Unit.Application.services;

[TestFixture]
[TestOf(typeof(ProjectInitialisingService))]
public class ProjectInitialisingServiceTest
{
    private Mock<IProjectRepository> _projectRepository = null!;
    private Mock<IImageRepository> _imageRepository = null!;
    private Mock<IProjectMetadataService> _projectMetadataService = null!;
    private Mock<ICollectionMetadataService> _collectionMetadataService = null!;
    private Mock<IFiles> _files = null!;
    private Mock<ILogger<ProjectInitialisingService>> _logger = null!;
    private ProjectInitialisingService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _projectRepository = new Mock<IProjectRepository>();
        _imageRepository = new Mock<IImageRepository>();
        _projectMetadataService = new Mock<IProjectMetadataService>();
        _collectionMetadataService = new Mock<ICollectionMetadataService>();
        _files = new Mock<IFiles>();
        _logger = new Mock<ILogger<ProjectInitialisingService>>();

        _service = new ProjectInitialisingService(
            _projectRepository.Object,
            _imageRepository.Object,
            _projectMetadataService.Object,
            CreateConfiguration(),
            _logger.Object,
            _files.Object,
            _collectionMetadataService.Object
        );
    }

    [Test]
    public void InitialiseFolder_ProjectFolder_InitialisesProject()
    {
        const string projectDirectory = @"C:\2024-07-04-Merijn";
        const string projectInfoPath = @"C:\2024-07-04-Merijn\project.info";

        // Mocks
        _files.Setup(f => f.GetPathEnd(projectDirectory))
            .Returns("2024-07-04-Merijn");

        _files.Setup(f => f.Combine(projectDirectory, Constants.ProjectInfoFile))
            .Returns(projectInfoPath);

        _files.Setup(f => f.Exists(projectInfoPath))
            .Returns(false);

        _files.Setup(f => f.GetDirectories(projectDirectory))
            .Returns([]);

        _projectRepository.Setup(r => r.Insert(It.IsAny<Project>()))
            .Returns((Project project) => new Project(
                project.Name,
                project.Path,
                project.EventDate,
                project.ParentProjectId,
                2
            ));

        // Execution
        _service.InitialiseFolder(projectDirectory);

        // Asserts
        _projectRepository.Verify(r => r.Insert(It.Is<Project>(p =>
            p.Id == null &&
            p.Name == "Merijn" &&
            p.Path == projectDirectory &&
            p.EventDate == new DateOnly(2024, 7, 4) &&
            p.ParentProjectId == null
        )), Times.Once);

        _files.Verify(f => f.WriteAllText("2", projectInfoPath), Times.Once);
    }

    [Test]
    public void InitialiseFolder_ProjectFolder_SingleDigitMonthAndDay_InitialisesProject()
    {
        const string projectDirectory = @"C:\2024-7-4-Merijn";
        const string projectInfoPath = @"C:\2024-7-4-Merijn\project.info";

        // Mocks
        _files.Setup(f => f.GetPathEnd(projectDirectory))
            .Returns("2024-7-4-Merijn");

        _files.Setup(f => f.Combine(projectDirectory, Constants.ProjectInfoFile))
            .Returns(projectInfoPath);

        _files.Setup(f => f.Exists(projectInfoPath))
            .Returns(false);

        _files.Setup(f => f.GetDirectories(projectDirectory))
            .Returns([]);

        _projectRepository.Setup(r => r.Insert(It.IsAny<Project>()))
            .Returns((Project project) => new Project(
                project.Name,
                project.Path,
                project.EventDate,
                project.ParentProjectId,
                2
            ));

        // Execution
        _service.InitialiseFolder(projectDirectory);

        // Asserts
        _projectRepository.Verify(r => r.Insert(It.Is<Project>(p =>
            p.Name == "Merijn" &&
            p.Path == projectDirectory &&
            p.EventDate == new DateOnly(2024, 7, 4)
        )), Times.Once);

        _files.Verify(f => f.WriteAllText("2", projectInfoPath), Times.Once);
    }

    [Test]
    public void InitialiseFolder_ProjectFolder_WithSpacesInName_InitialisesProject()
    {
        const string projectDirectory = @"C:\2024-07-04-Merijn gymnasium";
        const string projectInfoPath = @"C:\2024-07-04-Merijn gymnasium\project.info";

        // Mocks
        _files.Setup(f => f.GetPathEnd(projectDirectory))
            .Returns("2024-07-04-Merijn gymnasium");

        _files.Setup(f => f.Combine(projectDirectory, Constants.ProjectInfoFile))
            .Returns(projectInfoPath);

        _files.Setup(f => f.Exists(projectInfoPath))
            .Returns(false);

        _files.Setup(f => f.GetDirectories(projectDirectory))
            .Returns([]);

        _projectRepository.Setup(r => r.Insert(It.IsAny<Project>()))
            .Returns((Project project) => new Project(
                project.Name,
                project.Path,
                project.EventDate,
                project.ParentProjectId,
                2
            ));

        // Execution
        _service.InitialiseFolder(projectDirectory);

        // Asserts
        _projectRepository.Verify(r => r.Insert(It.Is<Project>(p =>
            p.Name == "Merijn gymnasium" &&
            p.Path == projectDirectory &&
            p.EventDate == new DateOnly(2024, 7, 4)
        )), Times.Once);

        _files.Verify(f => f.WriteAllText("2", projectInfoPath), Times.Once);
    }

    [Test]
    public void InitialiseFolder_AlreadyInitialisedProject_DoesNotInsertAgain()
    {
        const string projectDirectory = @"C:\2024-07-04-Merijn";
        const string projectInfoPath = @"C:\2024-07-04-Merijn\project.info";

        // Mocks
        _files.Setup(f => f.GetPathEnd(projectDirectory))
            .Returns("2024-07-04-Merijn");

        _files.Setup(f => f.Combine(projectDirectory, Constants.ProjectInfoFile))
            .Returns(projectInfoPath);

        _files.Setup(f => f.Exists(projectInfoPath))
            .Returns(true);

        _files.Setup(f => f.ReadAllText(projectInfoPath))
            .Returns("2");

        // Execution
        _service.InitialiseFolder(projectDirectory);

        // Asserts
        _projectRepository.Verify(r => r.Insert(It.IsAny<Project>()), Times.Never);
        _files.Verify(f => f.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _files.Verify(f => f.GetDirectories(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void InitialiseFolder_CollectionFolder_InitialisesProject()
    {
        const string collectionPath = @"C:\.Merijn";
        const string projectPath = @"C:\.Merijn\2024-07-04-Merijn";
        const string projectInfoPath = @"C:\.Merijn\2024-07-04-Merijn\project.info";

        // Mocks
        _files.Setup(f => f.GetPathEnd(collectionPath))
            .Returns(".Merijn");

        _files.Setup(f => f.GetPathEnd(projectPath))
            .Returns("2024-07-04-Merijn");

        _files.Setup(f => f.Combine(projectPath, Constants.ProjectInfoFile))
            .Returns(projectInfoPath);

        _files.Setup(f => f.Exists(projectInfoPath))
            .Returns(false);

        _files.Setup(f => f.GetDirectories(collectionPath))
            .Returns([projectPath]);

        _files.Setup(f => f.GetDirectories(projectPath))
            .Returns([]);

        _collectionMetadataService.Setup(s => s.GetCollectionMetadataConfiguration(collectionPath))
            .Returns((CollectionMetadataConfiguration?)null);

        _projectRepository.Setup(r => r.Insert(It.IsAny<Project>()))
            .Returns((Project project) => new Project(
                project.Name,
                project.Path,
                project.EventDate,
                project.ParentProjectId,
                2
            ));

        // Execution
        _service.InitialiseFolder(collectionPath);

        // Asserts
        _projectRepository.Verify(r => r.Insert(It.Is<Project>(p =>
            p.Id == null &&
            p.Name == "Merijn" &&
            p.Path == projectPath &&
            p.EventDate == new DateOnly(2024, 7, 4) &&
            p.ParentProjectId == null
        )), Times.Once);

        _files.Verify(f => f.WriteAllText("2", projectInfoPath), Times.Once);
    }

    [Test]
    public void InitialiseFolder_CollectionFolder_AddsCollectionMetadataToProject()
    {
        const string collectionPath = @"C:\.Merijn";
        const string projectPath = @"C:\.Merijn\2024-07-04-Merijn";
        const string projectInfoPath = @"C:\.Merijn\2024-07-04-Merijn\project.info";

        CollectionMetadataConfiguration collectionMetadataConfiguration = new()
        {
            MetadataKey = "client",
            MetadataValue = "Merijn"
        };

        // Mocks
        _files.Setup(f => f.GetPathEnd(collectionPath))
            .Returns(".Merijn");

        _files.Setup(f => f.GetPathEnd(projectPath))
            .Returns("2024-07-04-Merijn");

        _files.Setup(f => f.Combine(projectPath, Constants.ProjectInfoFile))
            .Returns(projectInfoPath);

        _files.Setup(f => f.Exists(projectInfoPath))
            .Returns(false);

        _files.Setup(f => f.GetDirectories(collectionPath))
            .Returns([projectPath]);

        _files.Setup(f => f.GetDirectories(projectPath))
            .Returns([]);

        _collectionMetadataService.Setup(s => s.GetCollectionMetadataConfiguration(collectionPath))
            .Returns(collectionMetadataConfiguration);

        _projectRepository.Setup(r => r.Insert(It.IsAny<Project>()))
            .Returns((Project project) => new Project(
                project.Name,
                project.Path,
                project.EventDate,
                project.ParentProjectId,
                2
            ));

        // Execution
        _service.InitialiseFolder(collectionPath);

        // Asserts
        _projectMetadataService.Verify(s => s.AddMetadataToProject(
            2,
            "client",
            "Merijn"
        ), Times.Once);
    }

    [Test]
    public void InitialiseFolder_EmptyFolder()
    {
        const string folderPath = @"C:\Merijn";

        // Mocks
        _files.Setup(f => f.GetPathEnd(folderPath))
            .Returns("Merijn");

        _files.Setup(f => f.GetDirectories(folderPath))
            .Returns([]);

        // Execution
        _service.InitialiseFolder(folderPath);

        // Asserts
        _projectRepository.Verify(r => r.Insert(It.IsAny<Project>()), Times.Never);
    }

    [Test]
    public void InitialiseFolder_Folder_WithEmptyFolder()
    {
        const string folderPath = @"C:\Merijn";
        const string subFolderPath = @"C:\Merijn\Tijdelijk";

        // Mocks
        _files.Setup(f => f.GetPathEnd(folderPath))
            .Returns("Merijn");

        _files.Setup(f => f.GetPathEnd(subFolderPath))
            .Returns("Tijdelijk");

        _files.Setup(f => f.GetDirectories(folderPath))
            .Returns([subFolderPath]);

        _files.Setup(f => f.GetDirectories(subFolderPath))
            .Returns([]);

        // Execution
        _service.InitialiseFolder(folderPath);

        // Asserts
        _projectRepository.Verify(r => r.Insert(It.IsAny<Project>()), Times.Never);
    }

    [Test]
    public void InitialiseProjectFolder_WithSubProjectFolder_InitialisesSubProjectAutomatically()
    {
        const string parentProjectDirectory = @"C:\2024-07-04-Merijn";
        const string parentProjectInfoPath = @"C:\2024-07-04-Merijn\project.info";

        const string subProjectDirectory = @"C:\2024-07-04-Merijn\.Ceremony";
        const string subProjectInfoPath = @"C:\2024-07-04-Merijn\.Ceremony\project.info";

        // Mocks
        _files.Setup(f => f.GetPathEnd(parentProjectDirectory))
            .Returns("2024-07-04-Merijn");

        _files.Setup(f => f.GetPathEnd(subProjectDirectory))
            .Returns(".Ceremony");

        _files.Setup(f => f.Combine(parentProjectDirectory, Constants.ProjectInfoFile))
            .Returns(parentProjectInfoPath);

        _files.Setup(f => f.Combine(subProjectDirectory, Constants.ProjectInfoFile))
            .Returns(subProjectInfoPath);

        _files.Setup(f => f.Exists(parentProjectInfoPath))
            .Returns(false);

        _files.Setup(f => f.Exists(subProjectInfoPath))
            .Returns(false);

        _files.Setup(f => f.GetDirectories(parentProjectDirectory))
            .Returns([subProjectDirectory]);

        _files.Setup(f => f.GetDirectories(subProjectDirectory))
            .Returns([]);

        int nextId = 10;

        _projectRepository.Setup(r => r.Insert(It.IsAny<Project>()))
            .Returns((Project project) => new Project(
                project.Name,
                project.Path,
                project.EventDate,
                project.ParentProjectId,
                nextId++
            ));

        // Execution
        _service.InitialiseFolder(parentProjectDirectory);

        // Asserts
        _projectRepository.Verify(r => r.Insert(It.Is<Project>(p =>
            p.Id == null &&
            p.Name == "Merijn" &&
            p.Path == parentProjectDirectory &&
            p.EventDate == new DateOnly(2024, 7, 4) &&
            p.ParentProjectId == null
        )), Times.Once);

        _projectRepository.Verify(r => r.Insert(It.Is<Project>(p =>
            p.Id == null &&
            p.Name == "Ceremony" &&
            p.Path == subProjectDirectory &&
            p.EventDate == new DateOnly(2024, 7, 4) &&
            p.ParentProjectId == 10
        )), Times.Once);

        _files.Verify(f => f.WriteAllText("10", parentProjectInfoPath), Times.Once);
        _files.Verify(f => f.WriteAllText("11", subProjectInfoPath), Times.Once);
    }

    [Test]
    public void InitialiseProject_ProjectFolderWithSubFolders_InitialisesProject()
    {
        const string projectDirectory = @"C:\2024-07-04-Merijn";
        const string projectInfoPath = @"C:\2024-07-04-Merijn\project.info";
        const string projectSubFolder = @"C:\2024-07-04-Merijn\Empty";

        // Mocks
        _files.Setup(f => f.GetPathEnd(projectDirectory))
            .Returns("2024-07-04-Merijn");

        _files.Setup(f => f.GetPathEnd(projectSubFolder))
            .Returns("Empty");

        _files.Setup(f => f.Combine(projectDirectory, Constants.ProjectInfoFile))
            .Returns(projectInfoPath);

        _files.Setup(f => f.Exists(projectInfoPath))
            .Returns(false);

        _files.Setup(f => f.GetDirectories(projectDirectory))
            .Returns([projectSubFolder]);

        _files.Setup(f => f.GetFiles(projectSubFolder))
            .Returns([]);

        _projectRepository.Setup(r => r.Insert(It.IsAny<Project>()))
            .Returns((Project project) => new Project(
                project.Name,
                project.Path,
                project.EventDate,
                project.ParentProjectId,
                2
            ));

        // Execution
        _service.InitialiseFolder(projectDirectory);

        // Asserts
        _projectRepository.Verify(r => r.Insert(It.Is<Project>(p =>
            p.Id == null &&
            p.Name == "Merijn" &&
            p.Path == projectDirectory &&
            p.EventDate == new DateOnly(2024, 7, 4) &&
            p.ParentProjectId == null
        )), Times.Once);

        _files.Verify(f => f.WriteAllText("2", projectInfoPath), Times.Once);
    }

    [Test]
    public void InitialiseProject_ProjectFolderWithConfiguredFolder_AddsFolderMetadata()
    {
        const string projectDirectory = @"C:\2024-07-04-Merijn";
        const string projectInfoPath = @"C:\2024-07-04-Merijn\project.info";
        const string originalsDirectory = @"C:\2024-07-04-Merijn\Originals";

        // Mocks
        _files.Setup(f => f.GetPathEnd(projectDirectory))
            .Returns("2024-07-04-Merijn");

        _files.Setup(f => f.GetPathEnd(originalsDirectory))
            .Returns("Originals");

        _files.Setup(f => f.Combine(projectDirectory, Constants.ProjectInfoFile))
            .Returns(projectInfoPath);

        _files.Setup(f => f.Exists(projectInfoPath))
            .Returns(false);

        _files.Setup(f => f.GetDirectories(projectDirectory))
            .Returns([originalsDirectory]);

        _files.Setup(f => f.GetFiles(originalsDirectory))
            .Returns([]);

        _projectRepository.Setup(r => r.Insert(It.IsAny<Project>()))
            .Returns((Project project) => new Project(
                project.Name,
                project.Path,
                project.EventDate,
                project.ParentProjectId,
                2
            ));

        // Execution
        _service.InitialiseFolder(projectDirectory);

        // Asserts
        _projectMetadataService.Verify(s => s.AddMetadataToProject(
            2,
            $"{Constants.FolderMetadataKeyPrefix}originals",
            "Originals"
        ), Times.Once);
    }

    [Test]
    public void InitializeImages_InsertsImagesWithRelativePaths()
    {
        const string projectDirectory = @"C:\2024-07-04-Merijn";
        const string originalsDirectory = @"C:\2024-07-04-Merijn\Originals";
        const int projectId = 2;

        const string filePath = @"C:\2024-07-04-Merijn\Originals\DSC_1234.NEF";

        // Mocks
        _files.Setup(f => f.GetFiles(originalsDirectory))
            .Returns([filePath]);

        _files.Setup(f => f.GetFileExtension(filePath))
            .Returns(".NEF");

        _files.Setup(f => f.GetFileName(filePath))
            .Returns("DSC_1234.NEF");

        _files.Setup(f => f.GetRelativePath(projectDirectory, filePath))
            .Returns(@"Originals\DSC_1234.NEF");

        // Execution
        _service.InitializeImages(projectDirectory, originalsDirectory, projectId);

        // Asserts
        _imageRepository.Verify(r => r.Insert(It.Is<Image>(i =>
            i.ProjectId == projectId &&
            i.FileName == "DSC_1234" &&
            i.FileType == ".NEF" &&
            i.RelationalFilePath == @"Originals\DSC_1234.NEF"
        )), Times.Once);
    }

    [Test]
    public void CreateProjectFolder()
    {
        // Mocks

        // Execution & Assert
        Assert.Throws<NotImplementedException>(() => _service.CreateProjectFolder());
        
        // Asserts
    }

    [Test]
    public void UpdateProjectFolder()
    {
        // Mocks

        // Execution & Assert
        Assert.Throws<NotImplementedException>(() => _service.UpdateProjectFolder());
        
        // Asserts
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{Constants.FolderNamesConfigKey}:Originals:0"] = "Originals",
                [$"{Constants.FolderNamesConfigKey}:Editing:0"] = "Editing",
                [$"{Constants.FolderNamesConfigKey}:Finals:0"] = "Finals"
            })
            .Build();
    }
}