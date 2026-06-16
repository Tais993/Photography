using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Application.interfaces.imageviewers;

public class ImageViewerGatewayHelper
{
    private readonly ILogger<ImageViewerGatewayHelper> _logger;

    public ImageViewerGatewayHelper(ILogger<ImageViewerGatewayHelper> logger)
    {
        _logger = logger;
    }

    public bool IsOpen(params string[] processNames)
    {
        return GetProcesses(processNames).Any();
    }

    public string? GetOpenedFile(string[] processNames, string[] titleSeparators)
    {
        List<Process> processes = GetProcesses(processNames);

        foreach (Process process in processes)
        {
            _logger.LogDebug("Main window title: {MainWindowTitle}", process.MainWindowTitle);

            string? fileName = ExtractFileNameFromTitle(process.MainWindowTitle, titleSeparators);

            if (fileName != null)
            {
                _logger.LogInformation("Found an opened file: {fileName}", fileName);
                return fileName;
            }
        }

        return null;
    }

    /// <summary>
    /// Opens a file using the default application configured in Windows for that file type.
    /// </summary>
    /// <param name="imageViewerPath"></param>
    /// <param name="imagePath">The path of the file that should be opened.</param>
    public ProcessStartInfo CreateFileOpenProcess(string imageViewerPath, string imagePath)
    {
        return new ProcessStartInfo
        {
            FileName = imageViewerPath,
            Arguments = $"\"{imagePath}\"",
            UseShellExecute = true
        };
    }

    /// <summary>
    /// Extracts the filename from a window title by removing the image viewer application name.
    /// </summary>
    /// <param name="title">The window title of the image viewer process.</param>
    /// <param name="titleSeparators">The possible separators between the filename and the application name.</param>
    /// <returns>The extracted filename when possible, otherwise null.</returns>
    private string? ExtractFileNameFromTitle(string? title, string[] titleSeparators)
    {
        if (string.IsNullOrEmpty(title))
        {
            return null;
        }

        if (titleSeparators.Length == 0)
        {
            return title;
        }

        foreach (string titleSeparator in titleSeparators)
        {
            int index = title.IndexOf(titleSeparator, StringComparison.OrdinalIgnoreCase);

            if (index > 0)
            {
                return title[..index].Trim();
            }
        }

        return null;
    }

    /// <summary>
    /// Gets all running processes that match one of the provided process names.
    /// </summary>
    /// <param name="processNames">The process names that should be searched for.</param>
    /// <returns>A list of matching running processes.</returns>
    private List<Process> GetProcesses(string[] processNames)
    {
        return Process.GetProcesses()
            .Where(process => processNames.Any(processName =>
                process.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }
}