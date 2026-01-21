using Microsoft.EntityFrameworkCore;
using PSFR_Repository.Context;
using PSFR_Repository.Entities;
using PSFR_Repository.Enums;
using PSFR_Repository.Models;
using PSFR_Repository.Repository.Interfaces;

namespace PSFR_Repository.Repository
{
    public class LookupItemRepository(DMSCustomContext context) : ILookupItemRepository
    {
        private readonly DMSCustomContext _context = context;
        const string collation = "Latin1_General_CS_AS";

        public async Task<List<ListResult>> GetItemsByListNameAndParentIdAsync(string? listName, int? parentId, Language language, string? searchTerm, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(listName)) return [];

            var query = _context.LookupItems
                .AsNoTracking()
                .Where(x => x.Lookup != null && x.Lookup.Name.ToLower() == listName);

            if (parentId.HasValue)
                query = query.Where(x => x.LookupItemParentId == parentId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string loweredSearch = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    !string.IsNullOrEmpty(x.Name) && x.Name.ToLower().Contains(loweredSearch)
                    || !string.IsNullOrEmpty(x.NameFr) && x.NameFr.ToLower().Contains(loweredSearch)
                    || !string.IsNullOrEmpty(x.NameAr) && x.NameAr.ToLower().Contains(loweredSearch));
            }
            return language switch
            {
                Language.FR => await query
                    .Select(x => new ListResult { Id = x.Id, Text = string.IsNullOrWhiteSpace(x.NameFr) ? x.Name : x.NameFr, Code = x.Code })
                    .OrderBy(x => x.Text)
                    .ToListAsync(cancellationToken),

                Language.AR => await query
                    .Select(x => new ListResult { Id = x.Id, Text = string.IsNullOrWhiteSpace(x.NameAr) ? x.Name : x.NameAr, Code = x.Code })
                    .OrderBy(x => x.Text)
                    .ToListAsync(cancellationToken),

                _ => await query
                    .Select(x => new ListResult { Id = x.Id, Text = x.Name, Code = x.Code })
                    .OrderBy(x => x.Text)
                    .ToListAsync(cancellationToken)
            };
        }
        public async Task<int?> GetLookupItemIdAsync(string? listName, string? itemName, CancellationToken cancellationToken = default)
        {
            LookupItems? lookupItem = await _context.LookupItems
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x != null && x.Lookup != null && x.Lookup.Name.ToLower() == listName && x.Name.Trim() == itemName, cancellationToken);

            return lookupItem?.Id;
        }
        public async Task<bool> CheckUniqueAsync(int? id, string name, string? nameAr, string? nameFr, string? code, short? parentId, CancellationToken cancellationToken = default)
        {
            IQueryable<LookupItems> query = _context.LookupItems.AsNoTracking();

            if (id.HasValue)
            {
                query = query.Where(item => item.Id != id.Value);
            }

            query = query.Where(item => item.ParentId == parentId);

            return await query.AnyAsync(item =>
                EF.Functions.Collate(item.Name, collation) == name
                || !string.IsNullOrWhiteSpace(code) && EF.Functions.Collate(item.Code, collation) == code
                || !string.IsNullOrWhiteSpace(nameAr) && !string.IsNullOrWhiteSpace(item.NameAr) && EF.Functions.Collate(item.NameAr, collation) == nameAr
                || !string.IsNullOrWhiteSpace(nameFr) && !string.IsNullOrWhiteSpace(item.NameFr) && EF.Functions.Collate(item.NameFr, collation) == nameFr, cancellationToken);
        }
        public async Task<LookupItems?> FindAsync(long id, CancellationToken cancellationToken = default)
        {
            return await _context.LookupItems
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == (int)id, cancellationToken);
        }
        public async Task InsertAsync(LookupItems item, CancellationToken cancellationToken = default)
        {
            item.CreatedDate = DateTime.Now;
            _context.LookupItems.Add(item);
            await _context.SaveChangesAsync(cancellationToken);
        }
        public async Task UpdateAsync(LookupItems item, CancellationToken cancellationToken = default)
        {
            item.ModifiedDate = DateTime.Now;
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
