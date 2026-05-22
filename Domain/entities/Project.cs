namespace Domain.entities;

public class Project(int? id, string name, string path, DateOnly eventDate) : IEntity
{
    public DateOnly EventDate = eventDate;
    public int? Id { get; } = id;
    public string Name { get; set; } = name;
    public string Path { get; } = path;

    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, Path: {Path}, EventDate: {EventDate}";
    }
}