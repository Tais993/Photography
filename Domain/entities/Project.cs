namespace Domain.entities;

public class Project(int? id, string name, string location, DateOnly eventDate) : IEntity
{
    public DateOnly EventDate = eventDate;
    public int? Id { get; } = id;
    public string Name { get; set; } = name;
    public string Location { get; } = location;

    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, Location: {Location}, EventDate: {EventDate}";
    }
}