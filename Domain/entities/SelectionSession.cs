namespace Domain.entities;

public class SelectionSession : IEntity
{
    public SelectionSession(int? id, int projectId, string name, List<int>? imageIds)
    {
        Id = id;
        ProjectId = projectId;
        Name = name;
        ImageIds = imageIds ?? [];
    }

    public int? Id { get; }
    public int ProjectId { get; }
    public string Name { get; }
    public List<int> ImageIds { get; }
}