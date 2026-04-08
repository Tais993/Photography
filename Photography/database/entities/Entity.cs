namespace PhotographyNET.database.entities;

public abstract class Entity(int id)
{
    public int Id { get; } = id;
}