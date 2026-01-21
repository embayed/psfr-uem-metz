using PSFR_Repository.Models;

namespace PSFR_Repository.Services.Interfaces
{
    public interface IMetadataTreeService
    {
        Task<List<MetadataTreeNodeDto>> GetLevelAsync(int fileContentTypeId, List<string> orderedFields, List<string> path, long userId, int roleId, List<short> groupIds, CancellationToken cancellationToken);
    }
}
