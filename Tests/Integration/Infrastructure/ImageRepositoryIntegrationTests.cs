using Application.interfaces;
using Domain.entities;
using Domain.entities.search;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration.Fixtures;

namespace Tests.Integration.Infrastructure;

public class ImageRepositoryIntegrationTests : IntegrationTestBase
{
    [Test]
    public void Insert_ThenGetById_ReturnsImage()
    {
        using IServiceScope scope = CreateScope();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();


        // Project setup
        Project project = projectRepository.Insert(CreateProject());

        // Setup
        string fileName = "DSC_1000";
        string fileType = ".png";
        string filePath = @"\Originals\DSC_1000.png";
        Image image = CreateImage(
            projectId: (int)project.Id!,
            fileName: fileName,
            fileType: fileType,
            relationalFilePath: filePath
        );

        // Execution
        Image insertedImage = imageRepository.Insert(image);
        Image retrievedImage = imageRepository.GetById((int)insertedImage.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(insertedImage, Is.Not.Null);
            Assert.That(retrievedImage, Is.Not.Null);

            Assert.That(retrievedImage.Id, Is.EqualTo(insertedImage.Id));
            Assert.That(retrievedImage.FileName, Is.EqualTo(fileName));
            Assert.That(retrievedImage.FileType, Is.EqualTo(fileType));
            Assert.That(retrievedImage.RelationalFilePath, Is.EqualTo(filePath));
            Assert.That(retrievedImage.ProjectId, Is.EqualTo(project.Id));
        }
    }

    [Test]
    public void InsertMultiple_GetAllByProjectId_ReturnsOnlyImagesForProject()
    {
        using IServiceScope scope = CreateScope();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();


        // Project setup
        Project firstProject = projectRepository.Insert(CreateProject());
        Project secondProject = projectRepository.Insert(CreateProject());

        // Setup
        string fileName = "DSC_1000";
        string fileType = ".png";
        string filePath = @"\Originals\DSC_1000.png";
        Image firstImage = CreateImage(
            projectId: (int)firstProject.Id!,
            fileName: fileName,
            fileType: fileType,
            relationalFilePath: filePath
        );

        Image secondImage = CreateImage(
            projectId: (int)secondProject.Id!);

        // Execution
        imageRepository.Insert(firstImage);
        imageRepository.Insert(secondImage);

        List<Image> projectImages = imageRepository.GetAllByProjectId((int)firstProject.Id!);

        // Asserts
        Assert.That(projectImages, Has.Count.EqualTo(1));
        Assert.That(projectImages[0].FileName, Is.EqualTo(firstImage.FileName));
        Assert.That(projectImages[0].FileType, Is.EqualTo(firstImage.FileType));
        Assert.That(projectImages[0].RelationalFilePath, Is.EqualTo(firstImage.RelationalFilePath));
    }

    [Test]
    public void InsertMultiple_GetAllByIds_ReturnsRequestedImages()
    {
        using IServiceScope scope = CreateScope();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();


        // Project setup
        Project project = projectRepository.Insert(CreateProject());

        // Setup
        string fileName = "DSC_1000";
        string fileType = ".png";
        string filePath = @"\Originals\DSC_1000.png";
        Image firstImage = CreateImage(
            projectId: (int)project.Id!,
            fileName: fileName,
            fileType: fileType,
            relationalFilePath: filePath
        );

        Image secondImage = CreateImage(
            projectId: (int)project.Id!);

        // Execution
        Image firstInsertedImage = imageRepository.Insert(firstImage);
        Image secondInsertedImage = imageRepository.Insert(secondImage);

        List<Image> images = imageRepository.GetAllByIds(
            [(int)firstInsertedImage.Id!, (int)secondInsertedImage.Id!]);


        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(images, Has.Count.EqualTo(2));
            Assert.That(images[0].Id, Is.EqualTo(firstInsertedImage.Id));
            Assert.That(images[1].Id, Is.EqualTo(secondInsertedImage.Id));
        }
    }

    [Test]
    public void GetProjectImageCount_ReturnsImageCountForProject()
    {
        using IServiceScope scope = CreateScope();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();


        // Project setup
        Project project = projectRepository.Insert(CreateProject());

        // Setup
        Image firstImage = CreateImage(projectId: (int)project.Id!);
        Image secondImage = CreateImage(projectId: (int)project.Id!);
        Image thirdImage = CreateImage(projectId: (int)project.Id!);

        // Execution
        imageRepository.Insert(firstImage);
        imageRepository.Insert(secondImage);
        imageRepository.Insert(thirdImage);

        int projectImageCount = imageRepository.GetProjectImageCount((int)project.Id!);

        // Asserts
        Assert.That(projectImageCount, Is.EqualTo(3));
    }

    [Test]
    public void SearchImages_ByFileName_ReturnsMatchingImage()
    {
        using IServiceScope scope = CreateScope();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();


        // Project setup
        Project project = projectRepository.Insert(CreateProject());

        // Setup
        Image firstImage = CreateImage(fileName: "DSC_1000");
        Image secondImage = CreateImage(fileName: "DSC_1110");
        Image thirdImage = CreateImage(fileName: "DSC_1001");
        Image fourthImage = CreateImage(fileName: "DSC_9282");

        ImageSearchSettings searchSettings = new ImageSearchSettings()
        {
            FileName = "DSC_9282",
        };

        // Execution
        imageRepository.Insert(firstImage);
        imageRepository.Insert(secondImage);
        imageRepository.Insert(thirdImage);
        imageRepository.Insert(fourthImage);

        List<Image> searchResult = imageRepository.SearchImages(searchSettings);

        // Asserts
        Assert.That(searchResult, Has.Count.EqualTo(1));
        Assert.That(searchResult[0].FileName, Is.EqualTo(fourthImage.FileName));
    }

    [Test]
    public void SearchImages_ByFileNumber_ReturnsMatchingImage()
    {
        using IServiceScope scope = CreateScope();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();


        // Project setup
        Project project = projectRepository.Insert(CreateProject());

        // Setup
        Image firstImage = CreateImage(fileName: "DSC_1000");
        Image secondImage = CreateImage(fileName: "DSC_1110");
        Image thirdImage = CreateImage(fileName: "DSC_1001");
        Image fourthImage = CreateImage(fileName: "DSC_9282");

        ImageSearchSettings searchSettings = new ImageSearchSettings()
        {
            FileNumber = "1000",
        };

        // Execution
        imageRepository.Insert(firstImage);
        imageRepository.Insert(secondImage);
        imageRepository.Insert(thirdImage);
        imageRepository.Insert(fourthImage);

        List<Image> searchResult = imageRepository.SearchImages(searchSettings);

        // Asserts
        Assert.That(searchResult, Has.Count.EqualTo(1));
        Assert.That(searchResult[0].FileName, Is.EqualTo(firstImage.FileName));
    }

    [Test]
    public void SearchImages_ByFileType_ReturnsMatchingImage()
    {
        using IServiceScope scope = CreateScope();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();


        // Project setup
        Project project = projectRepository.Insert(CreateProject());

        // Setup
        const string fileName = "DSC_1000";
        const string fileType = ".jpg";
        Image firstImage = CreateImage(fileName: fileName, fileType: fileType);
        Image secondImage = CreateImage(fileType: ".png");
        Image thirdImage = CreateImage(fileType: ".NEF");
        Image fourthImage = CreateImage(fileType: ".jpeg");

        ImageSearchSettings searchSettings = new ImageSearchSettings()
        {
            FileType = ".jpg",
        };

        // Execution
        imageRepository.Insert(firstImage);
        imageRepository.Insert(secondImage);
        imageRepository.Insert(thirdImage);
        imageRepository.Insert(fourthImage);

        List<Image> searchResult = imageRepository.SearchImages(searchSettings);

        // Asserts
        Assert.That(searchResult, Has.Count.EqualTo(1));
        Assert.That(searchResult[0].FileName, Is.EqualTo(fileName));
        Assert.That(searchResult[0].FileType, Is.EqualTo(fileType));
    }

    [Test]
    public void SearchImages_ByFolderName_ReturnsMatchingImage()
    {
        using IServiceScope scope = CreateScope();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();


        // Project setup
        Project project = projectRepository.Insert(CreateProject());

        // Setup
        const string fileName = "DSC_1000";
        const string relationalFilePath = @"Editing\DSC_1000.png";
        Image firstImage = CreateImage(fileName: fileName, relationalFilePath: relationalFilePath);
        Image secondImage = CreateImage(relationalFilePath: @"Originals\TestImage.png");
        Image thirdImage = CreateImage(relationalFilePath: @"Finals\TestImage.png");
        Image fourthImage = CreateImage(relationalFilePath: @"Weird\TestImage.png");

        ImageSearchSettings searchSettings = new ImageSearchSettings()
        {
            FolderName = "Editing",
        };

        // Execution
        imageRepository.Insert(firstImage);
        imageRepository.Insert(secondImage);
        imageRepository.Insert(thirdImage);
        imageRepository.Insert(fourthImage);

        List<Image> searchResult = imageRepository.SearchImages(searchSettings);

        // Asserts
        Assert.That(searchResult, Has.Count.EqualTo(1));
        Assert.That(searchResult[0].FileName, Is.EqualTo(fileName));
        Assert.That(searchResult[0].RelationalFilePath, Is.EqualTo(relationalFilePath));
    }

    [Test]
    public void DeleteById_RemovesImage()
    {
        using IServiceScope scope = CreateScope();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();


        // Project setup
        Project project = projectRepository.Insert(CreateProject());

        // Setup
        Image image = CreateImage(projectId: (int)project.Id!);

        // Execution
        Image insertedImage = imageRepository.Insert(image);
        int projectImageCountBeforeDeletion = imageRepository.GetProjectImageCount((int)project.Id!);

        imageRepository.DeleteById((int)insertedImage.Id!);

        Image? deletedImage = imageRepository.GetById((int)insertedImage.Id!);
        int projectImageCountAfterDeletion = imageRepository.GetProjectImageCount((int)project.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(deletedImage, Is.Null);
            Assert.That(projectImageCountBeforeDeletion, Is.EqualTo(1));
            Assert.That(projectImageCountAfterDeletion, Is.EqualTo(0));
        }
    }

    [Test]
    public void Update_ThenGetById_ReturnsUpdatedImage()
    {
        using IServiceScope scope = CreateScope();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();


        // Project setup
        Project project = projectRepository.Insert(CreateProject());

        // Setup
        const string fileName = "DSC_1000";
        const string fileType = ".png";
        const string relationalFilePath = @"\Originals\DSC_1000.png";
        Image image = CreateImage(
            projectId: (int)project.Id!,
            fileName: fileName,
            fileType: fileType,
            relationalFilePath: relationalFilePath
        );

        // Execution
        Image insertedImage = imageRepository.Insert(image);

        // Setup
        string updatedFileName = "DSC_1001";
        Image updatedImage = CreateImage(
            imageId: insertedImage.Id,
            fileName: updatedFileName,
            fileType: fileType,
            relationalFilePath: relationalFilePath
        );

        // Execution
        imageRepository.Update(updatedImage);
        Image retrievedImage = imageRepository.GetById((int)insertedImage.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(insertedImage, Is.Not.Null);
            Assert.That(retrievedImage, Is.Not.Null);


            Assert.That(insertedImage, Is.Not.Null);
            Assert.That(retrievedImage, Is.Not.Null);

            Assert.That(retrievedImage.Id, Is.EqualTo(insertedImage.Id));

            Assert.That(insertedImage.FileName, Is.EqualTo(fileName));
            Assert.That(retrievedImage.FileName, Is.EqualTo(updatedFileName));

            Assert.That(retrievedImage.FileType, Is.EqualTo(fileType));
            Assert.That(retrievedImage.RelationalFilePath, Is.EqualTo(relationalFilePath));

            Assert.That(retrievedImage.ProjectId, Is.EqualTo(project.Id));
        }
    }

    private static Project CreateProject(
        string name = "Test Project",
        string path = @"C:\Projects\Test Project",
        DateOnly? eventDate = null,
        int? parentProjectId = null)
    {
        return new Project(
            null,
            name,
            path,
            eventDate ?? new DateOnly(2026, 6, 17),
            parentProjectId);
    }

    private static Image CreateImage(
        int? imageId = null,
        int projectId = 1,
        string fileName = "TestImage",
        string fileType = ".png",
        string relationalFilePath = @"\Originals\TestImage.png")
    {
        return new Image(
            imageId,
            projectId,
            null,
            fileName,
            fileType,
            relationalFilePath
        );
    }
}