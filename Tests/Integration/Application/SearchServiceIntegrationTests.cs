using Application.interfaces.infrastructure;
using Application.interfaces.services;
using Domain.entities;
using Domain.entities.search;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration.Fixtures;
using static Tests.Integration.Utilities.IntegrationTestEntityFactory;

namespace Tests.Integration.Application;

public class SearchServiceIntegrationTests : IntegrationTestBase
{
    [Test]
    public void SearchImages_HideRawFilesWhenImageExists_HidesOnlyMatchingRawFiles()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository = scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository = scope.ServiceProvider.GetRequiredService<IImageRepository>();
        ISearchService searchService = scope.ServiceProvider.GetRequiredService<ISearchService>();

        // Setup
        Project project = projectRepository.Insert(CreateProject());

        imageRepository.Insert(CreateImage(projectId: (int)project.Id!, fileName: "DSC_0001", fileType: ".jpg"));
        imageRepository.Insert(CreateImage(projectId: (int)project.Id!, fileName: "DSC_0001", fileType: ".NEF"));

        imageRepository.Insert(CreateImage(projectId: (int)project.Id!, fileName: "DSC_0002", fileType: ".png"));
        imageRepository.Insert(CreateImage(projectId: (int)project.Id!, fileName: "DSC_0002", fileType: ".CR3"));

        imageRepository.Insert(CreateImage(projectId: (int)project.Id!, fileName: "DSC_0003", fileType: ".NEF"));

        ImageSearchSettings settings = new()
        {
            ProjectId = (int)project.Id!,
            HideRawFilesWhenImageExists = true,
            PageNumber = 1,
            PageSize = 20
        };

        // Execution
        PaginatedResult<Image> result = searchService.SearchImages(settings);
        List<Image> images = result.Items;

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.TotalItems, Is.EqualTo(3));
            Assert.That(images, Has.Count.EqualTo(3));

            Assert.That(images.Any(image =>
                image.FileName == "DSC_0001" &&
                image.FileType.Equals(".jpg", StringComparison.OrdinalIgnoreCase)), Is.True);

            Assert.That(images.Any(image =>
                image.FileName == "DSC_0002" &&
                image.FileType.Equals(".png", StringComparison.OrdinalIgnoreCase)), Is.True);

            Assert.That(images.Any(image =>
                image.FileName == "DSC_0003" &&
                image.FileType.Equals(".NEF", StringComparison.OrdinalIgnoreCase)), Is.True);

            Assert.That(images.Any(image =>
                image.FileName == "DSC_0001" &&
                image.FileType.Equals(".NEF", StringComparison.OrdinalIgnoreCase)), Is.False);

            Assert.That(images.Any(image =>
                image.FileName == "DSC_0002" &&
                image.FileType.Equals(".CR3", StringComparison.OrdinalIgnoreCase)), Is.False);
        }
    }

    [Test]
    public void SearchImages_FileNameOrNumber_SearchesByFileNameAndFileNumber()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository = scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository = scope.ServiceProvider.GetRequiredService<IImageRepository>();
        ISearchService searchService = scope.ServiceProvider.GetRequiredService<ISearchService>();

        // Setup
        Project project = projectRepository.Insert(CreateProject());

        imageRepository.Insert(CreateImage(
            projectId: (int)project.Id!,
            fileName: "DSC_0100",
            fileType: ".jpg"));

        imageRepository.Insert(CreateImage(
            projectId: (int)project.Id!,
            fileName: "DSC01234",
            fileType: ".jpg"));

        ImageSearchSettings fileNumberSearchSettings = new()
        {
            ProjectId = (int)project.Id!,
            FileNameOrNumber = "0100",
            PageNumber = 1,
            PageSize = 20
        };

        ImageSearchSettings fileNameSearchSettings = new()
        {
            ProjectId = (int)project.Id!,
            FileNameOrNumber = "DSC_0100",
            PageNumber = 1,
            PageSize = 20
        };

        // Execution
        PaginatedResult<Image> fileNumberResult = searchService.SearchImages(fileNumberSearchSettings);
        PaginatedResult<Image> fileNameResult = searchService.SearchImages(fileNameSearchSettings);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(fileNumberResult.TotalItems, Is.EqualTo(1));
            Assert.That(fileNumberResult.Items, Has.Count.EqualTo(1));
            Assert.That(fileNumberResult.Items.First().FileName, Is.EqualTo("DSC_0100"));

            Assert.That(fileNameResult.TotalItems, Is.EqualTo(1));
            Assert.That(fileNameResult.Items, Has.Count.EqualTo(1));
            Assert.That(fileNameResult.Items.First().FileName, Is.EqualTo("DSC_0100"));
        }
    }

    [Test]
    public void SearchImages_Pagination_ReturnsCorrectPageAndTotalItems()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository = scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository = scope.ServiceProvider.GetRequiredService<IImageRepository>();
        ISearchService searchService = scope.ServiceProvider.GetRequiredService<ISearchService>();

        // Setup
        Project project = projectRepository.Insert(CreateProject());

        imageRepository.Insert(CreateImage(projectId: (int)project.Id!, fileName: "DSC_0001", fileType: ".jpg"));
        imageRepository.Insert(CreateImage(projectId: (int)project.Id!, fileName: "DSC_0002", fileType: ".jpg"));
        imageRepository.Insert(CreateImage(projectId: (int)project.Id!, fileName: "DSC_0003", fileType: ".jpg"));

        ImageSearchSettings settings = new()
        {
            ProjectId = (int)project.Id!,
            PageNumber = 2,
            PageSize = 2
        };

        // Execution
        PaginatedResult<Image> result = searchService.SearchImages(settings);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.TotalItems, Is.EqualTo(3));
            Assert.That(result.PageNumber, Is.EqualTo(2));
            Assert.That(result.PageSize, Is.EqualTo(2));
            Assert.That(result.TotalPages, Is.EqualTo(2));
            Assert.That(result.Items, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public void SearchProjects_SearchAndPagination_ReturnsCorrectPageAndTotalItems()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository = scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        ISearchService searchService = scope.ServiceProvider.GetRequiredService<ISearchService>();

        // Setup
        projectRepository.Insert(CreateProject(name: "Holiday Sweden"));
        projectRepository.Insert(CreateProject(name: "Holiday Scotland"));
        projectRepository.Insert(CreateProject(name: "Concert Photos"));

        ProjectSearchSettings settings = new()
        {
            ProjectName = "Holiday",
            PageNumber = 1,
            PageSize = 1
        };

        // Execution
        PaginatedResult<Project> result = searchService.SearchProjects(settings);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.TotalItems, Is.EqualTo(2));
            Assert.That(result.PageNumber, Is.EqualTo(1));
            Assert.That(result.PageSize, Is.EqualTo(1));
            Assert.That(result.TotalPages, Is.EqualTo(2));
            Assert.That(result.Items, Has.Count.EqualTo(1));
            Assert.That(result.Items.First().Name, Does.Contain("Holiday"));
        }
    }
}