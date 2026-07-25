using Domain.entities;
using static Domain.utilities.ByteSizeFormatter;

namespace Cli.utils;

public abstract class CameraDriveSelector
{
    internal static void DisplayImportDrives(Dictionary<int, LogicalDrive> importDriveOptions)
    {
        Console.WriteLine("Found camera/import drives:");
        Console.WriteLine();

        foreach ((int number, LogicalDrive drive) in importDriveOptions)
        {
            string importDriveBadges = drive.GetImportDriveBadges();

            Console.WriteLine($"  [{number}] {drive.GetDisplayName()}");

            if (!string.IsNullOrEmpty(importDriveBadges))
            {
                Console.WriteLine($"      {importDriveBadges}");
            }

            Console.WriteLine($"      {FormatBytes(drive.TotalSize)} total · {FormatBytes(drive.TotalFreeSpace)} free");
            Console.WriteLine($"      Import folder: {drive.ImageFolderPath}");
            Console.WriteLine();
        }
    }

    internal static bool TryGetPickedDrive(Dictionary<int, LogicalDrive> importDriveOptions, out LogicalDrive pickedDrive)
    {
        pickedDrive = null!;

        Console.Write("Select drive: ");

        string? input = Console.ReadLine()?.Trim();

        if (!int.TryParse(input, out int selectedNumber) ||
            !importDriveOptions.TryGetValue(selectedNumber, out LogicalDrive? drive))
        {
            Console.WriteLine("Invalid drive selection.");
            return false;
        }

        pickedDrive = drive;
        return true;
    }
}