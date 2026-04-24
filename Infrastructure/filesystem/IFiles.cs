namespace Infrastructure.filesystem;

public interface IFiles
{
    /// <summary>
    /// Combines all given string into a path
    /// </summary>
    /// <param name="paths"></param>
    /// <returns></returns>
    string Combine(params string[] paths);

    /// <summary>
    /// Gets the filename from any given path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    string GetFileName(string path);

    /// <summary>
    /// Gets the final directory from any given path (if a filename is the last mentioned item, that will be returned instead)
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    string GetPathEnd(string path);

    /// <summary>
    /// Verifies whenever any given path exists on the drive, both directories and specific files work.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    bool Exists(string path);


    /// <summary>
    /// Returns all the subdirectories from any given path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    string[] GetDirectories(string path);

    /// <summary>
    /// Returns all files listed in the given path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    string[] GetFiles(string path);


    /// <summary>
    /// Creates and or writes all given text onto the given path
    /// </summary>
    /// <param name="text"></param>
    /// <param name="path"></param>
    void WriteAllText(string text, string path);

    /// <summary>
    /// Reads all text from a file on the given path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    string ReadAllText(string path);

    void DeleteFile(string path);



    /// <summary>
    /// Creates a folder at the given path
    /// </summary>
    /// <param name="path"></param>
    void FolderCreate(string path);

    /// <summary>
    /// Deletes an empty directory from the given path
    /// </summary>
    /// <param name="path"></param>
    void  FolderDelete(string path);




    /// <summary>
    /// Moves the given file from its origin path to its destination path.
    /// </summary>
    /// <param name="path">The current path</param>
    /// <param name="newPath">The new path</param>
    void MoveFile(string path, string newPath);

    /// <summary>
    /// Moves the given folder (or file) from its origin path to its destination path
    /// </summary>
    /// <param name="path">The current path</param>
    /// <param name="newPath">The new path</param>
    void MoveFolder(string path, string newPath);


}