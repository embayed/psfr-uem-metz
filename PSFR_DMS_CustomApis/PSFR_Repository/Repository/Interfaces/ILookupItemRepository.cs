using PSFR_Repository.Entities;
using PSFR_Repository.Enums;
using PSFR_Repository.Models;

namespace PSFR_Repository.Repository.Interfaces
{
    public interface ILookupItemRepository
    {
        Task<bool> CheckUniqueAsync(int? id, string name, string? nameAr, string? nameFr, string? code, short? parentId, CancellationToken cancellationToken = default);
        Task<LookupItems?> FindAsync(long id, CancellationToken cancellationToken);
        Task<List<ListResult>> GetItemsByListNameAndParentIdAsync(string? listName, int? parentId, Language language, string? searchTerm, CancellationToken cancellationToken = default);
        Task<int?> GetLookupItemIdAsync(string? listName, string? itemName, CancellationToken cancellationToken = default);
        Task InsertAsync(LookupItems item, CancellationToken cancellationToken = default);
        Task UpdateAsync(LookupItems item, CancellationToken cancellationToken = default);
    }
}
