namespace Domain.entities;

public class Project : IEntity
{
    public Project(int? id, string name, string path, DateOnly eventDate, int? parentProjectId)
    {
        EventDate = eventDate;
        Id = id;
        Name = name;
        Path = path;
        ParentProjectId = parentProjectId;
    }

    public Project(int? id, string name, string path, DateOnly eventDate)
    {
        EventDate = eventDate;
        Id = id;
        Name = name;
        Path = path;
    }

    public int? Id { get; }
    public string Name { get; set; }
    public string Path { get; }
    public DateOnly EventDate;
    public int? ParentProjectId { get; }

    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, Path: {Path}, EventDate: {EventDate},  ParentProjectId: {ParentProjectId}";
    }
}