namespace PhotographyNET.database.entities;

public class Project(int id, string name, string location) : Entity(id)
{

    public string Name { get; set; } = name;
    public string Location { get; } = location;

    public override string ToString()
    {
        return "Id: " + id + ", Name: " + name + ", Location: " + location;
    }
}