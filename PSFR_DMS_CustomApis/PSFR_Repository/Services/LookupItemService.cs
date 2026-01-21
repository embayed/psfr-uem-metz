using PSFR_Repository.Entities;
using PSFR_Repository.Enums;
using PSFR_Repository.Models;
using PSFR_Repository.Repository.Interfaces;
using PSFR_Repository.Services.Interfaces;

namespace PSFR_Repository.Services
{
    public class LookupItemService(ILookupItemRepository lookupItemRepository) : ILookupItemService
    {
        public async Task<List<ListResult>> GetListItemsByListItemParentAsync(string? parentListName, string? childListName, string? parentItemValue, Language language = Language.EN, string? searchTerm = null)
        {
            string? normalizedParentListName = parentListName?.Trim().ToLower();
            string? normalizedChildListName = childListName?.Trim().ToLower();
            string? normalizedParentItemValue = parentItemValue?.Trim();

            int? parentId = await lookupItemRepository.GetLookupItemIdAsync(normalizedParentListName, normalizedParentItemValue);
            return await lookupItemRepository.GetItemsByListNameAndParentIdAsync(normalizedChildListName, parentId, language, searchTerm);
        }

        public async Task<List<ListResult>> GetLookupItemsByNameAsync(string? listName, Language language = Language.EN, string? searchTerm = null)
        {
            string? normalizedListName = listName?.Trim().ToLower();
            return await lookupItemRepository.GetItemsByListNameAndParentIdAsync(normalizedListName, null, language, searchTerm);
        }

        public async Task<bool> CreateAsync(long userId, LookupItemsViewModel model, CancellationToken cancellationToken)
        {
            if (await lookupItemRepository.CheckUniqueAsync(model.Id, model.Name, model.NameAr, model.NameFr, model.Code, model.ParentId))
                return false;

            LookupItems newItem = new()
            {
                Name = model.Name,
                NameFr = model.NameFr,
                NameAr = model.NameAr,
                Code = model.Code,
                ParentId = model.ParentId,
                LookupItemParentId = model.LookupItemParentId,
                CreatedByUserId = userId
            };

            await lookupItemRepository.InsertAsync(newItem, cancellationToken);
            model.Id = newItem.Id;
            return true;
        }

        public async Task<bool> EditAsync(long userId, LookupItemsViewModel model, CancellationToken cancellationToken)
        {
            if (!model.Id.HasValue || !model.ParentId.HasValue || await lookupItemRepository.CheckUniqueAsync(model.Id, model.Name, model.NameAr, model.NameFr, model.Code, model.ParentId))
                return false;

            LookupItems? existingItem = await lookupItemRepository.FindAsync(model.Id.Value, cancellationToken);
            if (existingItem == null)
                return false;

            existingItem.Name = model.Name;
            existingItem.NameFr = model.NameFr;
            existingItem.NameAr = model.NameAr;
            existingItem.Code = model.Code;
            existingItem.ParentId = model.ParentId;
            existingItem.LookupItemParentId = model.LookupItemParentId;

            await lookupItemRepository.UpdateAsync(existingItem, cancellationToken);
            return true;
        }
    }
}