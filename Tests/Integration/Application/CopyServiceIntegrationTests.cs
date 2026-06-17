using Application.interfaces;
using Application.services.interfaces;
using Domain.entities;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration.Fixtures;
using static Tests.Integration.Utilities.IntegrationTestEntityFactory;

namespace Tests.Integration.Application;

public class CopyServiceIntegrationTests : StandardProjectIntegrationTestBase
{
    [Test]
    public void ImageIdsToRelativePaths_ReturnsRelativePathsForGivenImageIds()
    {
        using IServiceScope scope = CreateScope();

        ICopyService copyService = scope.ServiceProvider.GetRequiredService<ICopyService>();
        IProjectRepository projectRepository = scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository = scope.ServiceProvider.GetRequiredService<IImageRepository>();

        // Setup
        CreateStandardProjectDirectory();

        Project project = projectRepository.Insert(CreateProject(path: StandardProjectDirectory));

        Image firstImage = imageRepository.Insert(CreateImage(
            projectId: (int)project.Id!,
            fileName: "DSC_0001",
            fileType: ".jpg",
            relationalFilePath: Path.Combine(StandardOriginalFolderName, "DSC_0001.jpg")));

        Image secondImage = imageRepository.Insert(CreateImage(
            projectId: (int)project.Id!,
            fileName: "DSC_0002",
            fileType: ".jpg",
            relationalFilePath: Path.Combine(StandardOriginalFolderName, "DSC_0002.jpg")));

        imageRepository.Insert(firstImage);
        imageRepository.Insert(secondImage);

        int[] imageIds =
        [
            (int)firstImage.Id!,
            (int)secondImage.Id!
        ];

        // Execution
        List<string> relativePaths = copyService
            .ImageIdsToRelativePaths(imageIds)
            .ToList();

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(relativePaths, Has.Count.EqualTo(2));

            Assert.That(relativePaths, Does.Contain(
                Path.Combine(StandardOriginalFolderName, "DSC_0001.jpg")));

            Assert.That(relativePaths, Does.Contain(
                Path.Combine(StandardOriginalFolderName, "DSC_0002.jpg")));
        }
    }

    [Test]
    public void CopyFiles_WithRealFiles_CopiesFilesToTargetDirectory()
    {
        using IServiceScope scope = CreateScope();

        ICopyService copyService = scope.ServiceProvider.GetRequiredService<ICopyService>();

        // Setup
        CreateStandardProjectDirectory();

        CreateOriginalFile("DSC_0001.jpg", "First test image file");
        CreateOriginalFile("DSC_0002.jpg", "Second test image file");

        List<string> relativeFilePaths =
        [
            Path.Combine(StandardOriginalFolderName, "DSC_0001.jpg"),
            Path.Combine(StandardOriginalFolderName, "DSC_0002.jpg")
        ];

        // Execution
        copyService.CopyFiles(
            relativeFilePaths,
            StandardProjectDirectory,
            StandardEditingFolderName);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                TemporaryFileExists(
                    StandardProjectFolderName,
                    StandardEditingFolderName,
                    "DSC_0001.jpg"),
                Is.True);

            Assert.That(
                TemporaryFileExists(
                    StandardProjectFolderName,
                    StandardEditingFolderName,
                    "DSC_0002.jpg"),
                Is.True);

            Assert.That(
                ReadTemporaryFile(
                    StandardProjectFolderName,
                    StandardEditingFolderName,
                    "DSC_0001.jpg"),
                Is.EqualTo("First test image file"));

            Assert.That(
                ReadTemporaryFile(
                    StandardProjectFolderName,
                    StandardEditingFolderName,
                    "DSC_0002.jpg"),
                Is.EqualTo("Second test image file"));
        }
    }
}