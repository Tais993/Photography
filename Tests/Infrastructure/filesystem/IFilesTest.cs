using Infrastructure.filesystem;

namespace Tests.Infrastructure.filesystem;

[TestFixture]
[TestOf(typeof(Files))]
public class FilesTest
{
    private IFiles _files = null!;

    private const string FileExtension = ".txt";
    private const string FileName = "test.txt";
    private const string FilePath = @"C:\Users\Tijs\test.txt";

    private const string DirectoryEnd = "Tijs";
    private const string DirectoryPath = @"C:\Users\Tijs\";

    private const string SecondDirectoryPath = @"C:\Users\";

    [SetUp]
    public void SetUp()
    {
        _files = new Files();
    }

    [Test]
    public void CombinesPaths_Correctly()
    {
        const string firstPath = "C:\\Users";
        const string secondPath = "Tijs\\test.txt";

        string combinedPaths = _files.Combine(firstPath, secondPath);


        Assert.That(combinedPaths, Is.EqualTo(FilePath));
    }

    [Test]
    public void GetFileName_Correctly()
    {
        string fileName = _files.GetFileName(FilePath);

        Assert.That(fileName, Is.EqualTo(FileName));
    }

    [Test]
    public void GetfileExtension_Correctly()
    {
        string extension = _files.GetFileExtension(FilePath);

        Assert.That(extension, Is.EqualTo(FileExtension));
    }

    [Test]
    public void GetPathEnd_File_Correctly()
    {
        string pathEnd = _files.GetPathEnd(FilePath);

        Assert.That(pathEnd, Is.EqualTo(FileName));
    }

    [Test]
    public void GetPathEnd_Directory_Correctly()
    {
        string pathEnd = _files.GetPathEnd(DirectoryPath);

        Assert.That(pathEnd, Is.EqualTo(DirectoryEnd));
    }

    [Test]
    public void GetRelativePath_File_Correctly()
    {
        string relativePath = _files.GetRelativePath(SecondDirectoryPath, FilePath);

        Assert.That(relativePath, Is.EqualTo("Tijs\\test.txt"));
    }
}