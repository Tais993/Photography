using Application.interfaces;
using Application.services.interfaces;
using Domain.entities;
using Domain.entities.search;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration.Fixtures;
using static Tests.Integration.Utilities.IntegrationTestEntityFactory;

namespace Tests.Integration.Application;

public class SearchServiceIntegrationTests : IntegrationTestBase
{
    [Test]
    public void HideRawFilesWhenJpgExists()
    {
        using IServiceScope scope = CreateScope();
        ISearchService searchService = scope.ServiceProvider.GetRequiredService<ISearchService>();

        // Setup
        List<Image> images =
        [
            CreateImage(fileName: "DSC_0001"), CreateImage(fileName: "DSC_0001", fileType: ".NEF"),
            CreateImage(fileName: "DSC_0100"), CreateImage(fileName: "DSC_0100", fileType: ".NEF"),
            CreateImage(fileName: "DSC_0110", fileType:".jpg")
        ];

        // Execution
        List<Image> hiddenDuplicateRaws = searchService
            .HideRawFilesWhenNonRawExists(images)
            .ToList();

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(hiddenDuplicateRaws, Is.Not.Null);
            Assert.That(hiddenDuplicateRaws, Has.Count.EqualTo(3));

            // Visible files
            Assert.That(hiddenDuplicateRaws.Any(image =>
                image.FileName == "DSC_0001" &&
                image.FileType == ".png"), Is.True);

            Assert.That(hiddenDuplicateRaws.Any(image =>
                image.FileName == "DSC_0100" &&
                image.FileType == ".png"), Is.True);

            Assert.That(hiddenDuplicateRaws.Any(image =>
                image.FileName == "DSC_0110" &&
                image.FileType == ".jpg"), Is.True);

            // Hidden files
            Assert.That(hiddenDuplicateRaws.Any(image =>
                image.FileName == "DSC_0001" &&
                image.FileType == ".NEF"), Is.False);

            Assert.That(hiddenDuplicateRaws.Any(image =>
                image.FileName == "DSC_0100" &&
                image.FileType == ".NEF"), Is.False);
        }
    }
    
    [Test]
    public void Search_FileNameOrNumber_SearchesByFileNameAndFileNumber()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository = scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository = scope.ServiceProvider.GetRequiredService<IImageRepository>();
        ISearchService searchService = scope.ServiceProvider.GetRequiredService<ISearchService>();

        // Setup
        Project project = projectRepository.Insert(CreateProject());
        
        Image image = imageRepository.Insert(CreateImage(
            projectId: (int) project.Id!,
            fileName: "DSC_0100",
            fileType: ".jpg"));
        Image imageFake = imageRepository.Insert(CreateImage(
            projectId: (int) project.Id!,
            fileName: "DSC01234"));
        
        
        ImageSearchSettings fileNumberSearchSettings = new()
        {
            FileNameOrNumber = "0100"
        };

        ImageSearchSettings fileNameSearchSettings = new()
        {
            FileNameOrNumber = "DSC_0100"
        };

        // Execution
        PaginatedResult<Image> fileNumberResult = searchService.SearchImages(fileNumberSearchSettings);
        PaginatedResult<Image> fileNameResult = searchService.SearchImages(fileNameSearchSettings);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(fileNumberResult.Items, Has.Count.EqualTo(1));
            Assert.That(fileNumberResult.Items.First().FileName, Is.EqualTo("DSC_0100"));

            Assert.That(fileNameResult.Items, Has.Count.EqualTo(1));
            Assert.That(fileNameResult.Items.First().FileName, Is.EqualTo("DSC_0100"));
        }
    }
}