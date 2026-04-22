namespace Domain.entities;

public interface IIdEntity : IEntity
{
    public int? Id { get; }
}