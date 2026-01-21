using PSFR_Repository.Context;
using PSFR_Repository.Entities;
using PSFR_Repository.Repository.Interfaces;

namespace PSFR_Repository.Repository
{
    public class ExceptionLoggerRepository(DMSCustomContext context) : IExceptionLoggerRepository
    {
        private readonly DMSCustomContext _context = context;
        public async Task Insert(ExceptionLog exceptionLog, CancellationToken cancellationToken = default)
        {
            exceptionLog.CreatedDate = DateTime.Now;
            _context.ExceptionLogs.Add(exceptionLog);
            await _context.SaveChangesAsync();
        }
    }
}
