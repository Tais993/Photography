namespace Application.services.interfaces;

public interface ICopyService
{
    public void CopyFiles(IEnumerable<string> relativeFiles, string projectPath, string relativeTargetDirectory);
}