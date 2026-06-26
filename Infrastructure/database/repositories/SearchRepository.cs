using System.Text;
using Application.interfaces.infrastructure;
using Domain.entities;
using Domain.entities.search;
using Microsoft.Extensions.Logging;
using static Application.Constants;

namespace Infrastructure.database.repositories;

public class SearchRepository : ISearchRepository
{
    private readonly RepositoryHelper _db;
    private readonly ILogger<SearchRepository> _logger;

    public SearchRepository(
        RepositoryHelper db,
        ILogger<SearchRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public List<Image> SearchImages(ImageSearchSettings settings)
    {
        _logger.LogDebug("Searching images");

        List<object?> parameters = [];
        string whereSql = BuildImageWhereClause(settings, parameters);

        List<Image> images = GetPaginatedImages(settings, whereSql, parameters);

        _logger.LogDebug("Image search returned {Count} images", images.Count);

        return images;
    }

    public int CountImages(ImageSearchSettings settings)
    {
        _logger.LogDebug("Counting images");

        List<object?> parameters = [];
        string whereSql = BuildImageWhereClause(settings, parameters);

        return _db.QueryScalar<int>($"""
                                         SELECT COUNT(*)
                                         FROM public.image i
                                         {whereSql}
                                         """, parameters.ToArray());
    }

    public List<Project> SearchProjects(ProjectSearchSettings settings)
    {
        _logger.LogDebug("Searching projects");

        List<object?> parameters = [];
        string whereSql = BuildProjectWhereClause(settings, parameters);

        List<Project> projects = GetPaginatedProjects(settings, whereSql, parameters);

        _logger.LogDebug("Project search returned {Count} projects", projects.Count);

        return projects;
    }

    public int CountProjects(ProjectSearchSettings settings)
    {
        _logger.LogDebug("Counting projects");

        List<object?> parameters = [];
        string whereSql = BuildProjectWhereClause(settings, parameters);

        return _db.QueryScalar<int>($"""
                                     SELECT COUNT(*)
                                     FROM public.project p
                                     {whereSql}
                                     """, parameters.ToArray());
    }

    private static string BuildImageWhereClause(ImageSearchSettings settings, List<object?> parameters)
    {
        List<string> whereClauses = [];

        if (settings.ProjectId is not null)
        {
            parameters.Add(settings.ProjectId);
            whereClauses.Add($"i.project_id = ${parameters.Count}::int");
        }

        if (!string.IsNullOrWhiteSpace(settings.FileNumber))
        {
            parameters.Add(settings.FileNumber);
            whereClauses.Add($"i.file_name ~* ('(^|[^0-9])' || ${parameters.Count}::text || '([^0-9]|$)')");
        }

        if (!string.IsNullOrWhiteSpace(settings.FileName))
        {
            parameters.Add(settings.FileName);
            whereClauses.Add($"i.file_name ILIKE ('%' || ${parameters.Count}::text || '%')");
        }

        if (!string.IsNullOrWhiteSpace(settings.FolderName))
        {
            parameters.Add(settings.FolderName);
            whereClauses.Add($"replace(i.relational_file_path, chr(92), '/') LIKE (${parameters.Count}::text || '/%')");
        }

        if (!string.IsNullOrWhiteSpace(settings.FileType))
        {
            parameters.Add(settings.FileType);
            whereClauses.Add($"LOWER(i.file_type) = LOWER(${parameters.Count}::text)");
        }

        if (settings.ImageStatus is not null)
        {
            string imageStatus = ImageStatusMapper.ToDatabaseValue((ImageStatus) settings.ImageStatus);
            
            parameters.Add(imageStatus);
            whereClauses.Add($"LOWER(i.status) = LOWER(${parameters.Count}::text)");
        }

        
        AddHideRawFilesFilter(settings, parameters, whereClauses);

        return BuildWhereSql(whereClauses);
    }

    private static void AddHideRawFilesFilter(ImageSearchSettings settings, List<object?> parameters,
        List<string> whereClauses)
    {
        if (!settings.HideRawFilesWhenImageExists)
        {
            return;
        }

        parameters.Add(RawFileTypesSql);
        int rawFileTypesParameter = parameters.Count;

        parameters.Add(ImageFileTypesSql);
        int imageFileTypesParameter = parameters.Count;

        whereClauses.Add($"""
                          NOT (
                              LOWER(i.file_type) = ANY(${rawFileTypesParameter}::text[])
                              AND EXISTS (
                                  SELECT 1
                                  FROM public.image normal_image
                                  WHERE normal_image.project_id = i.project_id
                                    AND normal_image.file_name = i.file_name
                                    AND LOWER(normal_image.file_type) = ANY(${imageFileTypesParameter}::text[])
                              )
                          )
                          """);
    }

    private static string BuildProjectWhereClause(ProjectSearchSettings settings, List<object?> parameters)
    {
        List<string> whereClauses = [];

        if (settings.ProjectId is not null)
        {
            parameters.Add(settings.ProjectId);
            whereClauses.Add($"p.id = ${parameters.Count}::int");
        }

        if (!string.IsNullOrWhiteSpace(settings.ProjectName))
        {
            parameters.Add(settings.ProjectName);
            whereClauses.Add($"p.name ILIKE ('%' || ${parameters.Count}::text || '%')");
        }

        if (!string.IsNullOrWhiteSpace(settings.ProjectPath))
        {
            parameters.Add(settings.ProjectPath);
            whereClauses.Add($"p.path ILIKE ('%' || ${parameters.Count}::text || '%')");
        }

        if (settings.ParentProjectId is not null)
        {
            parameters.Add(settings.ParentProjectId);
            whereClauses.Add($"p.parent_project_id = ${parameters.Count}::int");
        }

        if (settings.EventDate is not null)
        {
            parameters.Add(settings.EventDate);
            whereClauses.Add($"p.event_date = ${parameters.Count}::date");
        }

        return BuildWhereSql(whereClauses);
    }

    private List<Image> GetPaginatedImages(ImageSearchSettings settings, string whereSql, List<object?> parameters)
    {
        List<object?> paginatedParameters = [..parameters];

        StringBuilder sqlBuilder = new();

        sqlBuilder.Append($"""
                           SELECT i.id,
                                  i.project_id,
                                  i.file_name,
                                  i.file_type,
                                  i.relational_file_path,
                                  i.status
                           FROM public.image i
                           {whereSql}
                           ORDER BY i.id DESC, i.file_name
                           """);

        AddPagination(sqlBuilder, settings, paginatedParameters);

        return _db.QueryMultiple(
            sqlBuilder.ToString(),
            DatabaseMappers.MapImage,
            paginatedParameters.ToArray());
    }

    private List<Project> GetPaginatedProjects(ProjectSearchSettings settings, string whereSql, List<object?> parameters)
    {
        List<object?> paginatedParameters = [..parameters];

        StringBuilder sqlBuilder = new();

        sqlBuilder.Append($"""
                           SELECT p.id,
                                  p.name,
                                  p.path,
                                  p.event_date,
                                  p.parent_project_id
                           FROM public.project p
                           {whereSql}
                           ORDER BY p.event_date DESC, p.name
                           """);

        AddPagination(sqlBuilder, settings, paginatedParameters);

        return _db.QueryMultiple(
            sqlBuilder.ToString(),
            DatabaseMappers.MapProject,
            paginatedParameters.ToArray());
    }

    private static string BuildWhereSql(List<string> whereClauses)
    {
        if (whereClauses.Count == 0)
        {
            return "";
        }

        return "\nWHERE " + string.Join("\n  AND ", whereClauses);
    }

    private static void AddPagination(StringBuilder sqlBuilder, SearchSettings settings, List<object?> parameters)
    {
        sqlBuilder.AppendLine();

        parameters.Add(settings.PageSize);
        sqlBuilder.AppendLine($"LIMIT ${parameters.Count}::int");

        int offset = (settings.PageNumber - 1) * settings.PageSize;

        parameters.Add(offset);
        sqlBuilder.AppendLine($"OFFSET ${parameters.Count}::int");
    }
}