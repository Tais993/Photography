namespace Domain.entities;

public class Project
{
    public Project(string name, string path, DateOnly eventDate, int? parentProjectId = null, int? id = null)
    {
        Name = name;
        Path = path;
        EventDate = eventDate;
        ParentProjectId = parentProjectId;
        Id = id;
    }

    public int? Id { get; }
    public string Name { get; set; }
    public string Path { get; }
    public DateOnly EventDate { get; }
    public int? ParentProjectId { get; }

    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, Path: {Path}, EventDate: {EventDate}, ParentProjectId: {ParentProjectId}";
    }
}