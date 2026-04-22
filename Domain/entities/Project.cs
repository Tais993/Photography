namespace Domain.entities;

public class Project(int? id, string name, string location, DateOnly eventDate) : IIdEntity
{
    public int? Id { get; } = id;
    public string Name { get; set; } = name;
    public string Location { get; } = location;
    public DateOnly EventDate = eventDate;

    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, Location: {Location}, EventDate: {EventDate}";
    }
}