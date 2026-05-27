namespace Application.services.interfaces;

public interface ICopyService
{
    public IEnumerable<string> ImageIdsToRelativePaths(int[] imageIds);
    
    public void CopyFiles(IEnumerable<string> relativeFiles, string projectPath, string relativeTargetDirectory);
}