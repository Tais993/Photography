using System.Diagnostics;
using System.Runtime.InteropServices;
using Application.interfaces.infrastructure;

namespace Infrastructure.filesystem;

public class Files : IFiles
{
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern uint GetCompressedFileSizeW(string lpFileName, out uint lpFileSizeHigh);


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

    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        return Directory.GetFiles(path, searchPattern, searchOption);
    }

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
    {
        return Directory.EnumerateFiles(path, searchPattern, searchOption);
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

    public long GetFolderSize(string path)
    {
        return EnumerateFiles(path, "*", SearchOption.AllDirectories)
            .Sum(GetFileSize);
    }

    public long GetLocalFolderSize(string path)
    {
        return EnumerateFiles(path, "*", SearchOption.AllDirectories)
            .Sum(GetLocalFileSize);
    }

    public long GetLocalFileSize(string path)
    {
        return OperatingSystem.IsWindows() ? GetWindowsLocalFileSize(path) : GetUnixLocalFileSize(path);
    }

    private long GetWindowsLocalFileSize(string path)
    {
        uint low = GetCompressedFileSizeW(path, out uint high);

        if (low == uint.MaxValue)
        {
            int error = Marshal.GetLastWin32Error();

            if (error != 0)
            {
                return GetFileSize(path);
            }
        }

        return ((long)high << 32) + low;
    }

    private long GetUnixLocalFileSize(string path)
    {
        return GetFileSize(path);
    }

    public long GetFileSize(string path)
    {
        return new FileInfo(path).Length;
    }
    
    public void OpenCommandLine(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/k",
                WorkingDirectory = path,
                UseShellExecute = true
            });

            return;
        }

        if (OperatingSystem.IsMacOS())
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "open",
                Arguments = $"-a Terminal \"{path}\"",
                UseShellExecute = false
            });

            return;
        }

        OpenLinuxTerminal(path);
    }

    private static void OpenLinuxTerminal(string path)
    {
        string[] terminals =
        [
            "x-terminal-emulator",
            "gnome-terminal",
            "konsole",
            "xfce4-terminal",
            "xterm"
        ];

        foreach (string terminal in terminals)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = terminal,
                    WorkingDirectory = path,
                    UseShellExecute = false
                });

                return;
            }
            catch
            {
                // ignored
            }
        }

        throw new InvalidOperationException("No supported terminal application was found.");
    }
}