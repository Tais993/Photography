namespace PhotographyNET.database.entities;

public interface IIdEntity : IEntity
{
    public int? Id { get; }
}