using static Application.Constants;

namespace Tests.Integration.Fixtures;

public abstract class StandardProjectIntegrationTestBase : FileSystemIntegrationTestBase
{
    protected const string DefaultProjectFolderName = "2026-06-17-TestProject";
    protected const string StandardOriginalFolderName = "Original";
    protected const string StandardEditingFolderName = "Editing";
    protected const string StandardFinalFolderName = "Final";

    protected string StandardProjectFolderName { get; private set; } = DefaultProjectFolderName;
    protected string StandardProjectDirectory { get; private set; } = null!;
    protected string StandardOriginalDirectory { get; private set; } = null!;
    protected string StandardEditingDirectory { get; private set; } = null!;
    protected string StandardFinalDirectory { get; private set; } = null!;

    protected string CreateStandardProjectDirectory(
        string projectFolderName = DefaultProjectFolderName)
    {
        StandardProjectFolderName = projectFolderName;

        StandardProjectDirectory = CreateDirectory(StandardProjectFolderName);

        StandardOriginalDirectory = CreateDirectory(
            StandardProjectFolderName,
            StandardOriginalFolderName);

        StandardEditingDirectory = CreateDirectory(
            StandardProjectFolderName,
            StandardEditingFolderName);

        StandardFinalDirectory = CreateDirectory(
            StandardProjectFolderName,
            StandardFinalFolderName);

        return StandardProjectDirectory;
    }

    protected string CreateOriginalFile(string fileName, string content = "")
    {
        EnsureStandardProjectDirectoryExists();

        return CreateFile(
            Path.Combine(
                StandardProjectFolderName,
                StandardOriginalFolderName,
                fileName),
            content);
    }

    protected string CreateEditingFile(string fileName, string content = "")
    {
        EnsureStandardProjectDirectoryExists();

        return CreateFile(
            Path.Combine(
                StandardProjectFolderName,
                StandardEditingFolderName,
                fileName),
            content);
    }

    protected string CreateFinalFile(string fileName, string content = "")
    {
        EnsureStandardProjectDirectoryExists();

        return CreateFile(
            Path.Combine(
                StandardProjectFolderName,
                StandardFinalFolderName,
                fileName),
            content);
    }

    protected void CreateStandardImagePair(string fileNameWithoutExtension = "DSC_0001")
    {
        CreateOriginalFile($"{fileNameWithoutExtension}.NEF");
        CreateOriginalFile($"{fileNameWithoutExtension}.JPG");
    }

    protected void CreateStandardRawFile(string fileNameWithoutExtension = "DSC_0001")
    {
        CreateOriginalFile($"{fileNameWithoutExtension}.NEF");
    }

    protected void CreateStandardJpgFile(string fileNameWithoutExtension = "DSC_0001")
    {
        CreateOriginalFile($"{fileNameWithoutExtension}.JPG");
    }

    protected string CreateSubProjectDirectory(string subProjectName)
    {
        EnsureStandardProjectDirectoryExists();

        string subProjectFolderName = GetSubProjectFolderName(subProjectName);

        string subProjectDirectory = CreateDirectory(
            StandardProjectFolderName,
            subProjectFolderName);

        CreateDirectory(
            StandardProjectFolderName,
            subProjectFolderName,
            StandardOriginalFolderName);

        CreateDirectory(
            StandardProjectFolderName,
            subProjectFolderName,
            StandardEditingFolderName);

        CreateDirectory(
            StandardProjectFolderName,
            subProjectFolderName,
            StandardFinalFolderName);

        return subProjectDirectory;
    }

    protected string GetSubProjectDirectory(string subProjectName)
    {
        EnsureStandardProjectDirectoryExists();

        return GetTemporaryPath(
            StandardProjectFolderName,
            GetSubProjectFolderName(subProjectName));
    }

    protected string CreateSubProjectOriginalFile(
        string subProjectName,
        string fileName,
        string content = "")
    {
        EnsureStandardProjectDirectoryExists();

        string subProjectFolderName = GetSubProjectFolderName(subProjectName);

        return CreateFile(
            Path.Combine(
                StandardProjectFolderName,
                subProjectFolderName,
                StandardOriginalFolderName,
                fileName),
            content);
    }

    protected void CreateSubProjectImagePair(
        string subProjectName,
        string fileNameWithoutExtension = "DSC_0002")
    {
        CreateSubProjectOriginalFile(subProjectName, $"{fileNameWithoutExtension}.NEF");
        CreateSubProjectOriginalFile(subProjectName, $"{fileNameWithoutExtension}.JPG");
    }

    protected string CreateProjectInfoFileInStandardProject(int projectId)
    {
        EnsureStandardProjectDirectoryExists();

        return CreateFile(
            Path.Combine(
                StandardProjectFolderName,
                ProjectInfoFile),
            projectId.ToString());
    }

    protected string CreateProjectInfoFileInSubProject(string subProjectName, int projectId)
    {
        EnsureStandardProjectDirectoryExists();

        string subProjectFolderName = GetSubProjectFolderName(subProjectName);

        return CreateFile(
            Path.Combine(
                StandardProjectFolderName,
                subProjectFolderName,
                ProjectInfoFile),
            projectId.ToString());
    }

    private static string GetSubProjectFolderName(string subProjectName)
    {
        return subProjectName.StartsWith(".")
            ? subProjectName
            : "." + subProjectName;
    }

    private void EnsureStandardProjectDirectoryExists()
    {
        if (!string.IsNullOrWhiteSpace(StandardProjectDirectory))
        {
            return;
        }

        CreateStandardProjectDirectory();
    }
}