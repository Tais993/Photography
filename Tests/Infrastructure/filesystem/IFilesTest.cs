using Application.interfaces;
using Infrastructure.filesystem;

namespace Tests.Infrastructure.filesystem;

[TestFixture]
[TestOf(typeof(Files))]
public class FilesTest
{
    private IFiles _files = null!;

    private const string FileExtension = ".txt";
    private const string FileName = "test.txt";
    
    private string _testRoot = null!;
    private string _filePath = null!;
    private string _directoryPath = null!;
    private string _secondDirectoryPath = null!;

    private const string DirectoryEnd = "Tijs";

    [SetUp]
    public void Setup()
    {
        _testRoot = Path.Combine(
            Path.GetTempPath(),
            "PictureProjectTests",
            Guid.NewGuid().ToString());

        _secondDirectoryPath = Path.Combine(
            _testRoot,
            "Users");

        _directoryPath = Path.Combine(
            _secondDirectoryPath,
            DirectoryEnd);

        _filePath = Path.Combine(
            _directoryPath,
            FileName);
        
        _files = new Files();
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testRoot))
        {
            Directory.Delete(_testRoot, true);
        }
    }

    [Test]
    public void CombinesPaths_Correctly()
    {
        string firstPath = _secondDirectoryPath;
        string secondPath = Path.Combine(DirectoryEnd, FileName);

        string combinedPaths = _files.Combine(firstPath, secondPath);

        Assert.That(combinedPaths, Is.EqualTo(_filePath));
    }

    [Test]
    public void GetFileName_Correctly()
    {
        string fileName = _files.GetFileName(_filePath);

        Assert.That(fileName, Is.EqualTo(FileName));
    }

    [Test]
    public void GetfileExtension_Correctly()
    {
        string extension = _files.GetFileExtension(_filePath);

        Assert.That(extension, Is.EqualTo(FileExtension));
    }

    [Test]
    public void GetPathEnd_File_Correctly()
    {
        string pathEnd = _files.GetPathEnd(_filePath);

        Assert.That(pathEnd, Is.EqualTo(FileName));
    }

    [Test]
    public void GetPathEnd_Directory_Correctly()
    {
        string pathEnd = _files.GetPathEnd(_directoryPath);

        Assert.That(pathEnd, Is.EqualTo(DirectoryEnd));
    }

    [Test]
    public void GetRelativePath_File_Correctly()
    {
        string relativePath = _files.GetRelativePath(_secondDirectoryPath, _filePath);

        Assert.That(relativePath, Is.EqualTo("Tijs" + Path.DirectorySeparatorChar + "test.txt"));
    }
}