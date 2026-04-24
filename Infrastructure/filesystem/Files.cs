namespace Infrastructure.filesystem;

public class Files : IFiles
{
    public string Combine(params string[] paths)
    {
        return Path.Combine(paths);
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

    public string[] GetFiles(string path)
    {
        return Directory.GetFiles(path);
    }

    public void WriteAllText(string text, string path)
    {
        File.WriteAllText(text, path);
    }

    public string ReadAllText(string path)
    {
        return File.ReadAllText(path);
    }

    public void DeleteFile(string path)
    {
        File.Delete(path);
    }

    public void FolderCreate(string path)
    {
        Directory.CreateDirectory(path);
    }

    public void FolderDelete(string path)
    {
        Directory.Delete(path);
    }

    public void MoveFile(string path, string newPath)
    {
        File.Move(path, newPath);
    }

    public void MoveFolder(string path, string newPath)
    {
        Directory.Move(path, newPath);
    }
}