using Domain.entities;
using Npgsql;

namespace Infrastructure.database.repositories;

public class DatabaseMappers
{
    internal static Image MapImage(NpgsqlDataReader reader)
    {
        return new Image(
            (int)reader["project_id"],
            (string)reader["file_name"],
            (string)reader["file_type"],
            (string)reader["relational_file_path"],
            (int)reader["id"]
        )
        {
            ImageStatus = ImageStatusMapper.ToImageStatus(reader["status"] as string)
        };
    }

    internal static Metadata MapMetadata(NpgsqlDataReader reader)
    {
        return new Metadata(
            (string)reader["metadata_key"],
            (string)reader["metadata_type"],
            (string)reader["display_name"],
            (string)reader["description"]
        );
    }

    public static ImageMetadata MapImageMetadata(NpgsqlDataReader reader)
    {
        return new ImageMetadata(
            (int)reader["image_id"],
            (string)reader["metadata_key"],
            (string)reader["metadata_value"],
            (string)reader["metadata_type"],
            (string)reader["display_name"],
            (string)reader["description"]
        );
    }

    internal static ProjectMetadata MapProjectMetadata(NpgsqlDataReader reader)
    {
        return new ProjectMetadata(
            (int)reader["project_id"],
            (string)reader["metadata_key"],
            (string)reader["metadata_value"],
            (string)reader["metadata_type"],
            (string)reader["display_name"],
            (string)reader["description"]
        );
    }

    internal static Project MapProject(NpgsqlDataReader reader)
    {
        int? parentProjectId = reader["parent_project_id"] == DBNull.Value
            ? null
            : (int)reader["parent_project_id"];


        return new Project(
            (string)reader["name"],
            (string)reader["path"],
            (DateOnly)reader["event_date"],
            reader["storage_total_bytes"] == DBNull.Value ? null : (long)reader["storage_total_bytes"],
            reader["storage_local_bytes"] == DBNull.Value ? null : (long)reader["storage_local_bytes"],
            reader["storage_last_calculated"] == DBNull.Value ? null : (DateTime)reader["storage_last_calculated"],
            reader["parent_project_id"] == DBNull.Value ? null : (int)reader["parent_project_id"],
            (int)reader["id"]
        );
    }

    internal static SelectionSession MapSelection(NpgsqlDataReader reader)
    {
        return new SelectionSession(
            (int)reader["project_id"],
            (string)reader["name"],
            ((int[])reader["image_ids"]).ToList(),
            (int)reader["id"]
        );
    }

    internal static SelectionSession MapSelectionWithoutImages(NpgsqlDataReader reader)
    {
        return new SelectionSession(
            (int)reader["project_id"],
            (string)reader["name"],
            [],
            (int)reader["id"]
        );
    }
}