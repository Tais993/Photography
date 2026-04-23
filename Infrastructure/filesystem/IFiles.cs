namespace Infrastructure.filesystem;

public interface IFiles
{
    string Combine(params string[] paths);
    string ReadAllText(string path);
    void WriteAllText(string text, string path);
    string GetFileName(string path);
    string GetPathEnd(string path);
    bool Exists(string path);
    string[] GetDirectories(string path);
}