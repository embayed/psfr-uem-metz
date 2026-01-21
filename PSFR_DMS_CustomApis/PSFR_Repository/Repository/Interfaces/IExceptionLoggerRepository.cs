using PSFR_Repository.Entities;

namespace PSFR_Repository.Repository.Interfaces
{
    public interface IExceptionLoggerRepository
    {
        Task Insert(ExceptionLog exceptionLog, CancellationToken cancellationToken = default);
    }
}
