namespace Domain.entities;

public class ProjectSearchSettings
{
    public DateOnly? EventDate;
    public int ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? ProjectPath { get; set; }
}