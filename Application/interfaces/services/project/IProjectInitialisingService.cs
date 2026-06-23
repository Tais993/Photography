using System.Text.RegularExpressions;
using Domain.entities;

namespace Application.interfaces.services.project;

public interface IProjectInitialisingService
{
    /// <summary>
    ///     Initialises the given folder into the database, it goes through all subfolders to recognize projects.
    ///     additionally this adds a file to the filesystem to remember the projects ID.
    ///     Any images from within the project will also be loaded in with its metadata saved into the database.
    ///     // TODO
    ///     Any collection folders are ignored for now, additionally if a subfolder was given its ignored as its not recognized
    ///     as a project folder.
    ///     In future versions this method will run recursively, including for any sub/collection folders.
    /// </summary>
    /// <param name="folderDirectory"></param>
    public void InitialiseFolder(string folderDirectory);
    
    
    /// <summary>
    /// 
    /// </summary>
    public void InitialiseProjectFolder(string projectDirectory, Match match, Project? parentProject);
    
    
    
    public void CreateProjectFolder();
    public void UpdateProjectFolder();
}