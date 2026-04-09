namespace PhotographyNET.database.entities;

public class Project(int? id, string name, string location, DateOnly eventDate) : Entity(id)
{

    public string Name { get; set; } = name;
    public string Location { get; } = location;
    public DateOnly EventDate = eventDate;

    public override string ToString()
    {
        return $"Id: {id}, Name: {name}, Location: {location}, EventDate: {EventDate}";
    }
}