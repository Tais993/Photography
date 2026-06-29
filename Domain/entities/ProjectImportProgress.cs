namespace Domain.entities;

public class ProjectImportProgress
{
    public Guid ImportId { get; set; }

    public int ProjectId { get; set; }

    public int TotalFiles { get; set; }
    public int FilesImported { get; set; }

    public string? CurrentFile { get; set; }

    public bool IsCompleted { get; set; }
    public bool HasFailed { get; set; }

    public string? ErrorMessage { get; set; }

    public int Percentage
    {
        get
        {
            if (TotalFiles == 0)
            {
                return 0;
            }

            return (int)Math.Round(FilesImported / (double)TotalFiles * 100);
        }
    }
}