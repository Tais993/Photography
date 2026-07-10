using Application.interfaces.infrastructure.repositories;
using Application.interfaces.services.project;
using Domain.entities;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration.Fixtures;
using static Tests.Integration.Utilities.IntegrationTestEntityFactory;

namespace Tests.Integration.Application.project;

public class ProjectFileScanningServiceIntegrationTests : StandardProjectIntegrationTestBase
{
    [Test]
    public void ScanProject_WithProjectImages_InsertsSupportedImagesOnly()
    {
        using IServiceScope scope = CreateScope();
        IProjectFileScanningService projectFileScanningService =
            scope.ServiceProvider.GetRequiredService<IProjectFileScanningService>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();

        // Setup
        CreateStandardProjectDirectory();
        CreateOriginalFile("DSC_0001.JPG", "First image");
        CreateOriginalFile("DSC_0002.NEF", "Second image");
        CreateEditingFile("EDIT_0001.png", "Edited image");
        CreateFinalFile("readme.txt", "Not an image");

        Project project = projectRepository.Insert(CreateProject(path: StandardProjectDirectory));

        // Execution
        projectFileScanningService.ScanProject(project, recursive: false, checkExistingImages: false);
        List<Image> images = imageRepository.GetAllByProjectId((int)project.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(images, Has.Count.EqualTo(3));

            Assert.That(
                images.Select(image => image.RelationalFilePath),
                Does.Contain(Path.Combine(StandardOriginalFolderName, "DSC_0001.JPG")));

            Assert.That(
                images.Select(image => image.RelationalFilePath),
                Does.Contain(Path.Combine(StandardOriginalFolderName, "DSC_0002.NEF")));

            Assert.That(
                images.Select(image => image.RelationalFilePath),
                Does.Contain(Path.Combine(StandardEditingFolderName, "EDIT_0001.png")));

            Assert.That(
                images.Select(image => image.RelationalFilePath),
                Does.Not.Contain(Path.Combine(StandardFinalFolderName, "readme.txt")));
        }
    }

    [Test]
    public void ScanProject_WithExistingMissingImage_MarksImageUnavailable()
    {
        using IServiceScope scope = CreateScope();
        IProjectFileScanningService projectFileScanningService =
            scope.ServiceProvider.GetRequiredService<IProjectFileScanningService>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();

        // Setup
        CreateStandardProjectDirectory();
        CreateOriginalFile("DSC_0001.JPG", "Existing image");

        Project project = projectRepository.Insert(CreateProject(path: StandardProjectDirectory));

        Image availableImage = imageRepository.Insert(CreateImage(
            projectId: (int)project.Id!,
            fileName: "DSC_0001",
            fileType: ".JPG",
            relationalFilePath: Path.Combine(StandardOriginalFolderName, "DSC_0001.JPG")));

        Image missingImage = imageRepository.Insert(CreateImage(
            projectId: (int)project.Id!,
            fileName: "DSC_9999",
            fileType: ".JPG",
            relationalFilePath: Path.Combine(StandardOriginalFolderName, "DSC_9999.JPG")));

        // Execution
        projectFileScanningService.ScanProject(project, recursive: false, checkExistingImages: true);

        Image? retrievedAvailableImage = imageRepository.GetById((int)availableImage.Id!);
        Image? retrievedMissingImage = imageRepository.GetById((int)missingImage.Id!);
        List<Image> images = imageRepository.GetAllByProjectId((int)project.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(images, Has.Count.EqualTo(2));

            Assert.That(retrievedAvailableImage, Is.Not.Null);
            Assert.That(retrievedAvailableImage!.ImageStatus, Is.EqualTo(ImageStatus.Available));

            Assert.That(retrievedMissingImage, Is.Not.Null);
            Assert.That(retrievedMissingImage!.ImageStatus, Is.EqualTo(ImageStatus.Unavailable));
        }
    }

    [Test]
    public void ScanProject_WithSubProject_ScansParentAndChildProjectImages()
    {
        using IServiceScope scope = CreateScope();
        IProjectFileScanningService projectFileScanningService =
            scope.ServiceProvider.GetRequiredService<IProjectFileScanningService>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();

        // Setup
        const string subProjectName = "Selection";

        CreateStandardProjectDirectory();
        CreateStandardImagePair("DSC_0001");

        CreateSubProjectDirectory(subProjectName);
        CreateSubProjectImagePair(subProjectName, "DSC_0002");

        Project parentProject = projectRepository.Insert(CreateProject(path: StandardProjectDirectory));
        Project subProject = projectRepository.Insert(CreateProject(
            name: subProjectName,
            path: GetSubProjectDirectory(subProjectName),
            parentProjectId: (int)parentProject.Id!));

        // Execution
        projectFileScanningService.ScanProject(parentProject, recursive: true, checkExistingImages: false);

        List<Image> parentImages = imageRepository.GetAllByProjectId((int)parentProject.Id!);
        List<Image> subProjectImages = imageRepository.GetAllByProjectId((int)subProject.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(parentImages, Has.Count.EqualTo(2));
            Assert.That(subProjectImages, Has.Count.EqualTo(2));

            Assert.That(
                parentImages.Select(image => image.FileName),
                Is.All.EqualTo("DSC_0001"));

            Assert.That(
                subProjectImages.Select(image => image.FileName),
                Is.All.EqualTo("DSC_0002"));
        }
    }
}