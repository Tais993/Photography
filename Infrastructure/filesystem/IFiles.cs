namespace Infrastructure.filesystem;

public interface IFiles
{

    string PathCombine(params string[] paths);
    string ReadFile(params string[] paths);
    void WriteFile(string text, params string[] paths);
    bool PathExists(params string[] paths);
    string[] ListFolder(params string[] paths);
}