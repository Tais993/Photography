namespace Domain.entities;

public class SelectionSession
{
    public SelectionSession(int projectId, string name, List<int>? imageIds = null, int? id = null)
    {
        ProjectId = projectId;
        Name = name;
        ImageIds = imageIds ?? [];
        Id = id;
    }

    public int? Id { get; }
    public int ProjectId { get; }
    public string Name { get; }
    public List<int> ImageIds { get; }
}