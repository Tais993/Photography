using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Infrastructure.irfanview;

public class IrfanViewRepository : IIrfanViewRepository
{
    private readonly ILogger<IrfanViewRepository> _logger;

    public IrfanViewRepository(ILogger<IrfanViewRepository> logger)
    {
        _logger = logger;
    }


    public bool IsOpen()
    {
        return GetIrfanviewProcesses().Any();
    }

    public string? GetOpenedFile()
    {
        List<Process> irfanViewProcesses = GetIrfanviewProcesses();

        foreach (Process irfanViewProcess in irfanViewProcesses)
        {
            _logger.LogDebug($"Main window title: {irfanViewProcess.MainWindowTitle}");

            string? fileName = ExtractFileNameFromTitle(irfanViewProcess.MainWindowTitle);

            if (fileName != null)
            {
                return fileName;
            }
        }

        return null;
    }

    private string? ExtractFileNameFromTitle(string? title)
    {
        if (string.IsNullOrEmpty(title))
        {
            return null;
        }

        const string separator = " - IrfanView";

        int index = title.IndexOf(separator, StringComparison.OrdinalIgnoreCase);

        if (index <= 0)
        {
            return null;
        }

        return title[..index].Trim();
    }

    private List<Process> GetIrfanviewProcesses()
    {
        return Process.GetProcesses()
            .Where(p =>
                p.ProcessName.Equals("i_view64", StringComparison.OrdinalIgnoreCase) ||
                p.ProcessName.Equals("i_view32", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}