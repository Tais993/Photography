namespace Domain.utilities;

public class ByteSizeFormatter
{
    public static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];

        double size = bytes;
        int unit = 0;

        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        if (unit == 0)
        {
            return $"{bytes} B";
        }

        return $"{size:0.#} {units[unit]}";
    }
}