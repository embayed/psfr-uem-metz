using Intalio.DMS.Domain.Entities;
using Intalio.DMS.Repository;
using Microsoft.EntityFrameworkCore;
using PSFR_Repository.Enums;
using PSFR_Repository.Models;
using PSFR_Repository.Services.Interfaces;
using System.Text.Json;

namespace PSFR_Repository.Services
{
    public sealed class MetadataTreeService(DMSContext context) : IMetadataTreeService
    {
        private readonly DMSContext _context = context;

        private readonly bool _isAdmin = false;

        public async Task<List<MetadataTreeNodeDto>> GetLevelAsync(int fileContentTypeId, List<string> orderedFields, List<string> path, long userId, int roleId, List<short> groupIds, CancellationToken cancellationToken)
        {
            if (orderedFields == null || orderedFields.Count == 0)
                throw new ArgumentException("orderedFields empty");

            groupIds ??= [];
            path ??= [];
            int levelIndex = path.Count;

            if (levelIndex > orderedFields.Count)
                levelIndex = orderedFields.Count;

            // Base query: only this content type + required navigation for permissions
            IQueryable<FileMetadata> query = _context.FileMetadata
                .Include(fm => fm.File)
                    .ThenInclude(f => f.FilePermissions)
                .Include(fm => fm.File.FolderParent)
                    .ThenInclude(fp => fp.PermissionFolder)
                        .ThenInclude(pf => pf.FolderPermissions)
                .Where(fm => fm.FileContentTypeId == fileContentTypeId);

            // Apply path-based JSON filters, but only within orderedFields bounds
            for (int i = 0; i < levelIndex; i++)
            {
                string key = orderedFields[i];

                if (i >= path.Count) break;
                string value = path[i];

                string encodedValue = EncodeJsonString(value);
                string pattern = $"\"{key}\":\"{encodedValue}\"";

                query = query.Where(fm => fm.FormData.Contains(pattern));
            }

            query = ApplyPermissionFilter(query, userId, roleId, groupIds);

            // If we reached (or exceeded) the last level, return files
            if (levelIndex >= orderedFields.Count)
            {
                List<MetadataTreeNodeDto> files = await query
                    .Select(fm => fm.File)
                    .Distinct()
                    .OrderBy(f => f.Name)
                    .Select(f => new MetadataTreeNodeDto
                    {
                        Label = f.Name + "." + f.Extension,
                        NodeType = MetadataTreeNodeType.File,
                        FileId = f.Id,
                        ItemsCount = 1,
                        HasChildren = false
                    })
                    .ToListAsync(cancellationToken);

                return files;
            }

            // Otherwise return folders for nextKey
            string nextKey = orderedFields[levelIndex];

            List<string> permittedFormData = await query
                .Select(fm => fm.FormData)
                .ToListAsync(cancellationToken);

            List<MetadataTreeNodeDto> folders = [.. permittedFormData
                .Select(fd => ExtractValue(fd, nextKey))
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .GroupBy(v => v)
                .Select(g => new MetadataTreeNodeDto
                {
                    Label = g.Key!,
                    NodeType = MetadataTreeNodeType.Folder,
                    FileId = null,
                    ItemsCount = g.Count(),
                    HasChildren = levelIndex + 1 < orderedFields.Count
                })
                .OrderBy(x => x.Label)];

            return folders;
        }

        private IQueryable<FileMetadata> ApplyPermissionFilter(IQueryable<FileMetadata> query, long userId, int roleId, List<short> groupIds)
        {
            if (_isAdmin)
                return query;

            return query.Where(fm => fm.File.IsBreakInheritance
            ? fm.File.FilePermissions.Any(p => p.UserId == userId || p.RoleId == roleId || p.GroupId.HasValue && groupIds.Contains(p.GroupId.Value))
            : !fm.File.FolderParent.PermissionFolderId.HasValue || fm.File.FolderParent.PermissionFolder.FolderPermissions.Any(p => (p.UserId == userId || p.RoleId == roleId || p.GroupId.HasValue && groupIds.Contains(p.GroupId.Value)) && !p.IndirectPermission));
        }

        private static string? ExtractValue(string json, string key)
        {
            if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(key))
                return null;

            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty(key, out JsonElement prop))
                    return prop.ToString();
            }
            catch
            {
                // keep silent to preserve existing behavior
            }

            return null;
        }

        private static string EncodeJsonString(string value)
        {
            string json = JsonSerializer.Serialize(value);

            // Strip quotes → "Métallerie" => Métallerie
            if (json.Length >= 2 && json[0] == '"' && json[^1] == '"')
                return json[1..^1];

            return json;
        }
    }
}
