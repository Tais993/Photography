namespace Infrastructure.filesystem;

public class Files : IFiles
{

    public string PathCombine(params string[] paths)
    {
        return Path.Combine(paths);
    }

    public string ReadFile(params string[] paths)
    {
        return File.ReadAllText(PathCombine(paths));
    }

    public void WriteFile(string text, params string[] paths)
    {
        File.WriteAllText(PathCombine(paths), text);
    }

    public bool PathExists(params string[] paths)
    {
        var projectInfoLocation = PathCombine(paths);

        return Path.Exists(projectInfoLocation);
    }

    public string[] ListFolder(params string[] paths)
    {
        return Directory.GetDirectories(PathCombine(paths));
    }
}