namespace PhotographyNET.database.entities;

public class Project(int? id, string name, string location, DateOnly date) : Entity(id)
{

    public string Name { get; set; } = name;
    public string Location { get; } = location;
    public DateOnly Date = date;

    public override string ToString()
    {
        return $"Id: {id}, Name: {name}, Location: {location}, Date: {Date}";
    }
}