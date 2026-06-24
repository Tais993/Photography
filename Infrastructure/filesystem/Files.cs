using Application.interfaces.infrastructure;

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

    public string GetFileNameWithoutExtension(string path)
    {
        return Path.GetFileNameWithoutExtension(path);
    }

    public string GetFileExtension(string path)
    {
        return Path.GetExtension(path);
    }

    public string GetPathEnd(string path)
    {
        return Path.GetFileName(
            path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        );
    }

    public string GetDirectoryName(string path)
    {
        return Path.GetDirectoryName(path);
    }

    public DirectoryInfo? GetParentDirectory(string path)
    {
        return Directory.GetParent(path);
    }

    public string GetFullPath(string path)
    {
        return Path.GetFullPath(path);
    }

    public string GetRelativePath(string relativeTo, string path)
    {
        return Path.GetRelativePath(relativeTo, path);
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
        File.WriteAllText(path, text);
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

    public void FolderDelete(string path, bool recursive = false)
    {
        Directory.Delete(path, recursive);
    }

    public void CopyFile(string path, string newPath)
    {
        File.Copy(path, newPath);
    }

    public void MoveFile(string path, string newPath)
    {
        File.Move(path, newPath);
    }

    public void MoveFolder(string path, string newPath)
    {
        Directory.Move(path, newPath);
        
    }
    
    public DateTime GetLastWriteTimeUtc(string path)
    {
        return File.GetLastWriteTimeUtc(path);
    }
}