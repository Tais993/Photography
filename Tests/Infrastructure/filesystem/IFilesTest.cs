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
    
    private readonly string _filePath = @"C:" + Path.DirectorySeparatorChar + "Users" + Path.DirectorySeparatorChar + 
                                       "Tijs" + Path.DirectorySeparatorChar + "test.txt";

    private const string DirectoryEnd = "Tijs";
    private readonly string _directoryPath = @"C:" + Path.DirectorySeparatorChar + "Users" + Path.DirectorySeparatorChar + "Tijs";

    private readonly string _secondDirectoryPath = @"C:" + Path.DirectorySeparatorChar + "Users" + Path.DirectorySeparatorChar;

    [SetUp]
    public void SetUp()
    {
        _files = new Files();
    }

    [Test]
    public void CombinesPaths_Correctly()
    {
        string firstPath = "C:" + Path.DirectorySeparatorChar + "Users";
        string secondPath = "Tijs" + Path.DirectorySeparatorChar +"test.txt";

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