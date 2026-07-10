using Application.interfaces.infrastructure.repositories;
using Application.services.project;
using Domain.entities;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration.Fixtures;
using static Tests.Integration.Utilities.IntegrationTestEntityFactory;

namespace Tests.Integration.Application.project;

public class ProjectUpdateServiceIntegrationTests : StandardProjectIntegrationTestBase
{
    [Test]
    public void UpdateProject_WithNewAndMissingFiles_UpdatesStorageAndImageStatuses()
    {
        using IServiceScope scope = CreateScope();
        ProjectUpdateService projectUpdateService =
            scope.ServiceProvider.GetRequiredService<ProjectUpdateService>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();

        // Setup
        CreateStandardProjectDirectory();
        CreateOriginalFile("DSC_0001.JPG", "New original image");
        CreateEditingFile("EDIT_0001.png", "New editing image");

        Project project = projectRepository.Insert(CreateProject(path: StandardProjectDirectory));

        Image missingImage = imageRepository.Insert(CreateImage(
            projectId: (int)project.Id!,
            fileName: "DSC_9999",
            fileType: ".JPG",
            relationalFilePath: Path.Combine(StandardOriginalFolderName, "DSC_9999.JPG")));

        // Execution
        projectUpdateService.UpdateProject(project);

        Project? updatedProject = projectRepository.GetById((int)project.Id!);
        Image? retrievedMissingImage = imageRepository.GetById((int)missingImage.Id!);
        List<Image> images = imageRepository.GetAllByProjectId((int)project.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(updatedProject, Is.Not.Null);
            Assert.That(updatedProject!.StorageTotalBytes, Is.Not.Null);
            Assert.That(updatedProject.StorageTotalBytes!.Value, Is.GreaterThan(0));
            Assert.That(updatedProject.StorageLocalBytes, Is.Not.Null);
            Assert.That(updatedProject.StorageLastCalculated, Is.Not.Null);

            Assert.That(images, Has.Count.EqualTo(3));

            Assert.That(
                images.Select(image => image.RelationalFilePath),
                Does.Contain(Path.Combine(StandardOriginalFolderName, "DSC_0001.JPG")));

            Assert.That(
                images.Select(image => image.RelationalFilePath),
                Does.Contain(Path.Combine(StandardEditingFolderName, "EDIT_0001.png")));

            Assert.That(retrievedMissingImage, Is.Not.Null);
            Assert.That(retrievedMissingImage!.ImageStatus, Is.EqualTo(ImageStatus.Unavailable));
        }
    }

    [Test]
    public void UpdateProject_WithChangedProjectPath_PersistsNewPathAndScansNewPath()
    {
        using IServiceScope scope = CreateScope();
        ProjectUpdateService projectUpdateService =
            scope.ServiceProvider.GetRequiredService<ProjectUpdateService>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();

        // Setup
        string oldProjectDirectory = CreateDirectory("2026-06-17-OldProject");

        CreateStandardProjectDirectory("2026-06-17-NewProject");
        CreateOriginalFile("DSC_0001.JPG", "Image in new location");

        Project project = projectRepository.Insert(CreateProject(path: oldProjectDirectory));

        // Execution
        projectUpdateService.UpdateProject(project, StandardProjectDirectory);

        Project? updatedProject = projectRepository.GetById((int)project.Id!);
        List<Image> images = imageRepository.GetAllByProjectId((int)project.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(updatedProject, Is.Not.Null);
            Assert.That(updatedProject!.Path, Is.EqualTo(StandardProjectDirectory));
            Assert.That(updatedProject.StorageTotalBytes, Is.Not.Null);
            Assert.That(updatedProject.StorageTotalBytes!.Value, Is.GreaterThan(0));

            Assert.That(images, Has.Count.EqualTo(1));
            Assert.That(
                images[0].RelationalFilePath,
                Is.EqualTo(Path.Combine(StandardOriginalFolderName, "DSC_0001.JPG")));
        }
    }

    [Test]
    public void UpdateProjectByPath_WithProjectInfoFileInParentFolder_ResolvesProjectAndScansImages()
    {
        using IServiceScope scope = CreateScope();
        ProjectUpdateService projectUpdateService =
            scope.ServiceProvider.GetRequiredService<ProjectUpdateService>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();

        // Setup
        CreateStandardProjectDirectory();
        CreateOriginalFile("DSC_0001.JPG", "Resolved project image");

        Project project = projectRepository.Insert(CreateProject(path: StandardProjectDirectory));
        CreateProjectInfoFileInStandardProject((int)project.Id!);

        // Execution
        projectUpdateService.UpdateProjectByPath(StandardProjectDirectory);
        List<Image> images = imageRepository.GetAllByProjectId((int)project.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(images, Has.Count.EqualTo(1));
            Assert.That(images[0].FileName, Is.EqualTo("DSC_0001"));
            Assert.That(
                images[0].RelationalFilePath,
                Is.EqualTo(Path.Combine(StandardOriginalFolderName, "DSC_0001.JPG")));
        }
    }

    [Test]
    public void UpdateProject_WithSubProject_UpdatesParentAndChildProjects()
    {
        using IServiceScope scope = CreateScope();
        ProjectUpdateService projectUpdateService =
            scope.ServiceProvider.GetRequiredService<ProjectUpdateService>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();

        // Setup
        const string subProjectName = "Selection";

        CreateStandardProjectDirectory();
        CreateOriginalFile("DSC_0001.JPG", "Parent image");

        CreateSubProjectDirectory(subProjectName);
        CreateSubProjectOriginalFile(subProjectName, "DSC_0002.JPG", "Subproject image");

        Project parentProject = projectRepository.Insert(CreateProject(path: StandardProjectDirectory));
        Project subProject = projectRepository.Insert(CreateProject(
            name: subProjectName,
            path: GetSubProjectDirectory(subProjectName),
            parentProjectId: (int)parentProject.Id!));

        // Execution
        projectUpdateService.UpdateProject(parentProject);

        Project? updatedParentProject = projectRepository.GetById((int)parentProject.Id!);
        Project? updatedSubProject = projectRepository.GetById((int)subProject.Id!);
        List<Image> parentImages = imageRepository.GetAllByProjectId((int)parentProject.Id!);
        List<Image> subProjectImages = imageRepository.GetAllByProjectId((int)subProject.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(updatedParentProject, Is.Not.Null);
            Assert.That(updatedParentProject!.StorageTotalBytes, Is.Not.Null);
            Assert.That(updatedParentProject.StorageLastCalculated, Is.Not.Null);

            Assert.That(updatedSubProject, Is.Not.Null);
            Assert.That(updatedSubProject!.StorageTotalBytes, Is.Not.Null);
            Assert.That(updatedSubProject.StorageLastCalculated, Is.Not.Null);

            Assert.That(parentImages, Has.Count.EqualTo(1));
            Assert.That(parentImages[0].FileName, Is.EqualTo("DSC_0001"));

            Assert.That(subProjectImages, Has.Count.EqualTo(1));
            Assert.That(subProjectImages[0].FileName, Is.EqualTo("DSC_0002"));
        }
    }
}