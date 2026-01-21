using PSFR_Repository.Enums;
using PSFR_Repository.Models;

namespace PSFR_Repository.Services.Interfaces
{
    public interface ILookupItemService
    {
        Task<bool> CreateAsync(long userId, LookupItemsViewModel model, CancellationToken cancellationToken);
        Task<bool> EditAsync(long userId, LookupItemsViewModel model, CancellationToken cancellationToken);
        Task<List<ListResult>> GetListItemsByListItemParentAsync(string parentListName, string childListName, string parentItemValue, Language Language = Language.EN, string? searchTerm = null);
        Task<List<ListResult>> GetLookupItemsByNameAsync(string ListName, Language Language = Language.EN, string? searchTerm = null);
    }
}
