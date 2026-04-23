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

    public bool Exists(string path)
    {
        return Path.Exists(path);
    }

    public string[] GetDirectories(string path)
    {
        return Directory.GetDirectories(path);
    }
}