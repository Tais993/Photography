using System.Diagnostics;
using Application.interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.irfanview;

public class IrfanViewGateway : IIrfanViewGateway
{
    private readonly ILogger<IrfanViewGateway> _logger;
    private readonly string? _irfanViewPath;

    public IrfanViewGateway(ILogger<IrfanViewGateway> logger, IConfiguration configuration)
    {
        _irfanViewPath = configuration.GetValue<string>("ImageViewer:IrfanViewPath");
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
    
    public void OpenFile(string filePath)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = _irfanViewPath,
            Arguments = $"\"{filePath}\"",
            UseShellExecute = false
        });
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