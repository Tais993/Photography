namespace Infrastructure.filesystem;

public class Files : IFiles
{

    public string Combine(params string[] paths)
    {
        return Path.Combine(paths);
    }

    public string ReadAllText(string path)
    {
        return File.ReadAllText(path);
    }

    public void WriteAllText(string text, string path)
    {
        File.WriteAllText(path, text);
    }

    public string GetFileName(string path)
    {
        return Path.GetFileName(path);
    }

    public string GetPathEnd(string path)
    {
        return Path.GetFileName(
            path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        );
    }

    public bool Exists(string path)
    {
        return Path.Exists(path);
    }

    public string[] GetDirectories(string path)
    {
        return Directory.GetDirectories(path);
    }
}