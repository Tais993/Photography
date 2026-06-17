using Application;

namespace Tests.Integration.Fixtures;

public abstract class FileSystemIntegrationTestBase : IntegrationTestBase
{
    protected string TemporaryDirectory { get; private set; } = null!;

    [SetUp]
    public void FileSystemSetUp()
    {
        TemporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            "PhotographyIntegrationTests",
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(TemporaryDirectory);
    }

    [TearDown]
    public void FileSystemTearDown()
    {
        if (!Directory.Exists(TemporaryDirectory))
        {
            return;
        }

        Directory.Delete(TemporaryDirectory, true);
    }

    protected string CreateDirectory(params string[] relativePathParts)
    {
        string directoryPath = GetTemporaryPath(relativePathParts);

        Directory.CreateDirectory(directoryPath);

        return directoryPath;
    }

    protected string CreateFile(string relativeFilePath, string content = "")
    {
        string filePath = GetTemporaryPath(relativeFilePath);

        string? directoryPath = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        File.WriteAllText(filePath, content);

        return filePath;
    }

    protected string CreateFile(string[] relativePathParts, string content = "")
    {
        string filePath = GetTemporaryPath(relativePathParts);

        string? directoryPath = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        File.WriteAllText(filePath, content);

        return filePath;
    }

    protected string CreateProjectInfoFile(int projectId)
    {
        return CreateFile(Constants.ProjectInfoFile, projectId.ToString());
    }

    protected string GetTemporaryPath(params string[] relativePathParts)
    {
        return Path.Combine(
            new[] { TemporaryDirectory }
                .Concat(relativePathParts)
                .ToArray());
    }

    protected bool TemporaryFileExists(params string[] relativePathParts)
    {
        return File.Exists(GetTemporaryPath(relativePathParts));
    }

    protected bool TemporaryDirectoryExists(params string[] relativePathParts)
    {
        return Directory.Exists(GetTemporaryPath(relativePathParts));
    }

    protected string ReadTemporaryFile(params string[] relativePathParts)
    {
        return File.ReadAllText(GetTemporaryPath(relativePathParts));
    }
}