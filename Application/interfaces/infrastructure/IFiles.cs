namespace Application.interfaces.infrastructure;

public interface IFiles
{
    /// <summary>
    ///     Combines all given string into a path
    /// </summary>
    /// <param name="paths"></param>
    /// <returns></returns>
    string Combine(params string[] paths);

    /// <summary>
    ///     Gets the filename from any given path, including the extension
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    string GetFileName(string path);

    /// <summary>
    ///     Gets the filename from any given path, excluding the extension
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    string GetFileNameWithoutExtension(string path);


    /// <summary>
    ///     Gets the extention of the given file's path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public string GetFileExtension(string path);

    /// <summary>
    ///     Gets the final directory from any given path (if a filename is the last mentioned item, that will be returned
    ///     instead)
    ///     If the given path included a filename at the end, the file namne including the extension will be returned.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    string GetPathEnd(string path);

    /// <summary>
    ///     Gets the full directory based on the given path, but it removes the filename.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public string GetDirectoryName(string path);

    /// <summary>
    ///     Gets the parent directory from the given path.
    ///     Returns null when the path has no parent directory.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    DirectoryInfo? GetParentDirectory(string path);


    /// <summary>
    ///     Resolves the full, absolute path based on the relative path it receives.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public string GetFullPath(string path);
    
    /// <summary>
    ///     Creates
    /// </summary>
    /// <param name="relativeTo"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public string GetRelativePath(string relativeTo, string path);

    /// <summary>
    ///     Verifies whenever any given path exists on the drive, both directories and specific files work.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    bool Exists(string path);


    /// <summary>
    ///     Returns all the subdirectories from any given path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    string[] GetDirectories(string path);

    /// <summary>
    ///     Returns all files listed in the given path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    string[] GetFiles(string path);


    /// <summary>
    ///     Creates and or writes all given text onto the given path
    /// </summary>
    /// <param name="text"></param>
    /// <param name="path"></param>
    void WriteAllText(string text, string path);

    /// <summary>
    ///     Reads all text from a file on the given path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    string ReadAllText(string path);

    /// <summary>
    /// Deletes a file from the given path.
    /// </summary>
    /// <param name="path"></param>
    void DeleteFile(string path);


    /// <summary>
    ///     Creates a folder at the given path
    /// </summary>
    /// <param name="path"></param>
    void FolderCreate(string path);

    /// <summary>
    ///     Deletes an empty directory from the given path
    /// </summary>
    /// <param name="path"></param>
    void FolderDelete(string path);


    /// <summary>
    ///     Copy the given file from its origin path to its destination path.
    /// </summary>
    /// <param name="path">The current path</param>
    /// <param name="newPath">The copied to path</param>
    void CopyFile(string path, string newPath);


    /// <summary>
    ///     Moves the given file from its origin path to its destination path.
    /// </summary>
    /// <param name="path">The current path</param>
    /// <param name="newPath">The new path</param>
    void MoveFile(string path, string newPath);

    /// <summary>
    ///     Moves the given folder (or file) from its origin path to its destination path
    /// </summary>
    /// <param name="path">The current path</param>
    /// <param name="newPath">The new path</param>
    void MoveFolder(string path, string newPath);


    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public DateTime GetLastWriteTimeUtc(string path);

}