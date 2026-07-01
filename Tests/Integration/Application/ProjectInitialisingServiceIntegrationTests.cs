using Application.interfaces.infrastructure;
using Application.interfaces.infrastructure.repositories;
using Application.interfaces.services.project;
using Domain.entities;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration.Fixtures;
using static Application.Constants;

namespace Tests.Integration.Application;

public class ProjectInitialisingServiceIntegrationTests : StandardProjectIntegrationTestBase
{

    [Test]
    public void InitialiseFolder_WithStandardProject_CreatesProjectAndImages()
    {
        using IServiceScope scope = CreateScope();
        IProjectInitialisingService projectInitialisingService =
            scope.ServiceProvider.GetRequiredService<IProjectInitialisingService>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();

        // Setup
        const string projectName = "TestProject";
        DateOnly projectDate = new DateOnly(2026, 6, 17);
        const string projectFolderName = "2026-06-17-TestProject";

        CreateStandardProjectDirectory(projectFolderName);
        CreateStandardImagePair();

        // Execution
        projectInitialisingService.InitialiseFolder(StandardProjectDirectory);

        List<Project> projects = projectRepository.GetAll();
        List<Image> images = imageRepository.GetAll();

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(projects, Has.Count.EqualTo(1));

            Assert.That(projects[0].Name, Is.EqualTo(projectName));
            Assert.That(projects[0].Path, Is.EqualTo(StandardProjectDirectory));
            Assert.That(projects[0].EventDate, Is.EqualTo(projectDate));
            Assert.That(projects[0].ParentProjectId, Is.Null);

            Assert.That(
                TemporaryFileExists(StandardProjectFolderName, ProjectInfoFile),
                Is.True);

            Assert.That(
                ReadTemporaryFile(StandardProjectFolderName, ProjectInfoFile),
                Is.EqualTo(projects[0].Id.ToString()));

            Assert.That(images, Has.Count.EqualTo(2));

            Assert.That(
                images.Select(image => image.ProjectId),
                Is.All.EqualTo(projects[0].Id));

            Assert.That(
                images.Select(image => image.FileName),
                Is.All.EqualTo("DSC_0001"));

            Assert.That(
                images.Select(image => image.FileType),
                Does.Contain(".NEF"));

            Assert.That(
                images.Select(image => image.FileType),
                Does.Contain(".JPG"));

            Assert.That(
                images.Select(image => image.RelationalFilePath),
                Does.Contain(Path.Combine("Original", "DSC_0001.NEF")));

            Assert.That(
                images.Select(image => image.RelationalFilePath),
                Does.Contain(Path.Combine("Original", "DSC_0001.JPG")));
        }
    }

    [Test]
    public void InitialiseFolder_WithAlreadyInitialisedProject_DoesNotCreateDuplicateProject()
    {
        using IServiceScope scope = CreateScope();
        IProjectInitialisingService projectInitialisingService =
            scope.ServiceProvider.GetRequiredService<IProjectInitialisingService>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();

        // Setup
        const string projectFolderName = "2026-06-17-TestProject";

        CreateStandardProjectDirectory(projectFolderName: projectFolderName);
        CreateStandardImagePair();

        // Execution
        projectInitialisingService.InitialiseFolder(StandardProjectDirectory);
        projectInitialisingService.InitialiseFolder(StandardProjectDirectory);

        List<Project> projects = projectRepository.GetAll();
        List<Image> images = imageRepository.GetAll();

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(projects, Has.Count.EqualTo(1));
            Assert.That(images, Has.Count.EqualTo(2));

            Assert.That(
                TemporaryFileExists(StandardProjectFolderName, ProjectInfoFile),
                Is.True);
        }
    }

    [Test]
    public void InitialiseFolder_WithParentFolder_InitialisesProjectSubdirectory()
    {
        using IServiceScope scope = CreateScope();
        IProjectInitialisingService projectInitialisingService =
            scope.ServiceProvider.GetRequiredService<IProjectInitialisingService>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();

        // Setup
        const string projectName = "TestProject";
        DateOnly projectDate = new DateOnly(2026, 6, 17);
        const string projectFolderName = "2026-06-17-TestProject";

        CreateStandardProjectDirectory(projectFolderName);
        CreateStandardImagePair("DSC_1234");

        // Execution
        projectInitialisingService.InitialiseFolder(TemporaryDirectory);

        List<Project> projects = projectRepository.GetAll();
        List<Image> images = imageRepository.GetAll();

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(projects, Has.Count.EqualTo(1));

            Assert.That(projects[0].Name, Is.EqualTo(projectName));
            Assert.That(projects[0].Path, Is.EqualTo(StandardProjectDirectory));
            Assert.That(projects[0].EventDate, Is.EqualTo(projectDate));
            Assert.That(projects[0].ParentProjectId, Is.Null);

            Assert.That(
                TemporaryFileExists(StandardProjectFolderName, ProjectInfoFile),
                Is.True);

            Assert.That(images, Has.Count.EqualTo(2));

            Assert.That(
                images.Select(image => image.ProjectId),
                Is.All.EqualTo(projects[0].Id));

            Assert.That(
                images.Select(image => image.FileName),
                Is.All.EqualTo("DSC_1234"));

            Assert.That(
                images.Select(image => image.FileType),
                Does.Contain(".NEF"));

            Assert.That(
                images.Select(image => image.FileType),
                Does.Contain(".JPG"));
        }
    }

    [Test]
    public void InitialiseFolder_WithSubProject_CreatesParentAndSubProject()
    {
        using IServiceScope scope = CreateScope();
        IProjectInitialisingService projectInitialisingService =
            scope.ServiceProvider.GetRequiredService<IProjectInitialisingService>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();

        // Setup
        const string projectFolderName = "2026-06-17-TestProject";
        const string parentProjectName = "TestProject";
        const string subProjectName = "Selection";
        DateOnly projectDate = new DateOnly(2026, 6, 17);

        CreateStandardProjectDirectory(projectFolderName: projectFolderName);
        CreateStandardImagePair("DSC_0001");

        CreateSubProjectDirectory(subProjectName);
        CreateSubProjectImagePair(subProjectName, "DSC_0002");

        // Execution
        projectInitialisingService.InitialiseFolder(StandardProjectDirectory);

        List<Project> projects = projectRepository.GetAll();
        List<Image> images = imageRepository.GetAll();

        Project parentProject = projects.Single(project => project.ParentProjectId is null);
        Project subProject = projects.Single(project => project.ParentProjectId is not null);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(projects, Has.Count.EqualTo(2));

            Assert.That(parentProject.Name, Is.EqualTo(parentProjectName));
            Assert.That(parentProject.Path, Is.EqualTo(StandardProjectDirectory));
            Assert.That(parentProject.EventDate, Is.EqualTo(projectDate));
            Assert.That(parentProject.ParentProjectId, Is.Null);

            Assert.That(subProject.Name, Is.EqualTo(subProjectName));
            Assert.That(subProject.Path, Is.EqualTo(GetSubProjectDirectory(subProjectName)));
            Assert.That(subProject.EventDate, Is.EqualTo(projectDate));
            Assert.That(subProject.ParentProjectId, Is.EqualTo(parentProject.Id));

            Assert.That(images, Has.Count.EqualTo(4));

            Assert.That(
                images.Count(image => image.ProjectId == parentProject.Id),
                Is.EqualTo(2));

            Assert.That(
                images.Count(image => image.ProjectId == subProject.Id),
                Is.EqualTo(2));

            Assert.That(
                images
                    .Where(image => image.ProjectId == parentProject.Id)
                    .Select(image => image.FileName),
                Is.All.EqualTo("DSC_0001"));

            Assert.That(
                images
                    .Where(image => image.ProjectId == subProject.Id)
                    .Select(image => image.FileName),
                Is.All.EqualTo("DSC_0002"));

            Assert.That(
                TemporaryFileExists(StandardProjectFolderName, ProjectInfoFile),
                Is.True);

            Assert.That(
                TemporaryFileExists(StandardProjectFolderName, ".Selection", ProjectInfoFile),
                Is.True);

            Assert.That(
                ReadTemporaryFile(StandardProjectFolderName, ProjectInfoFile),
                Is.EqualTo(parentProject.Id.ToString()));

            Assert.That(
                ReadTemporaryFile(StandardProjectFolderName, ".Selection", ProjectInfoFile),
                Is.EqualTo(subProject.Id.ToString()));
        }
    }
}