namespace Domain.entities;

public class Project
{
    public Project(string name, string path, DateOnly eventDate, int? parentProjectId = null, int? id = null)
    {
        Name = name;
        Path = path;
        EventDate = eventDate;
        ParentProjectId = parentProjectId;
        Id = id;
    }

    public Project(string name, string path, DateOnly eventDate, long? storageTotalBytes,
        long? storageLocalBytes, DateTime? storageLastCalculated, int? parentProjectId = null,
        int? id = null) : this(name, path, eventDate, parentProjectId: parentProjectId, id: id)
    {
        StorageTotalBytes = storageTotalBytes;
        StorageLocalBytes = storageLocalBytes;
        StorageLastCalculated = storageLastCalculated;
    }

    public int? Id { get; }
    public string Name { get; set; }
    public string Path { get; }
    public DateOnly EventDate { get; }
    public int? ParentProjectId { get; }
    public long? StorageTotalBytes { get; set; }
    public long? StorageLocalBytes { get; set; }
    public DateTime? StorageLastCalculated { get; set; }

    public override string ToString()
    {
        return
            $"Id: {Id}, Name: {Name}, Path: {Path}, EventDate: {EventDate}, ParentProjectId: {ParentProjectId}, StorageTotalBytes: {StorageTotalBytes}, StorageLocalBytes: {StorageLocalBytes}, StorageLastCalculated: {StorageLastCalculated}";
    }
}