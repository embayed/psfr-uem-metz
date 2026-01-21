using PSFR_Repository.Entities;
using PSFR_Repository.Enums;
using PSFR_Repository.Repository.Interfaces;
using PSFR_Repository.Services.Interfaces;

namespace PSFR_Repository.Services
{
    public class ExceptionLoggerService(IExceptionLoggerRepository exceptionLoggerRepository) : IExceptionLoggerService
    {

        public async Task LogExceptionAsync(Exception ex, long? userId = null, long? primaryKeyValue = null, LoggingLevel level = LoggingLevel.Error)
        {
            ExceptionLog exceptionLog = new()
            {
                CreatedDate = DateTime.Now,
                UserId = userId,
                Exception = ex.ToString(),
                PrimaryKeyValue = primaryKeyValue,
                MachineName = Environment.MachineName,
                Level = level.ToString()
            };
            await exceptionLoggerRepository.Insert(exceptionLog);
        }
        public async Task LogExceptionAsync(string message, long? userId = null, long? primaryKeyValue = null, LoggingLevel level = LoggingLevel.Error)
        {
            ExceptionLog exceptionLog = new()
            {
                CreatedDate = DateTime.Now,
                UserId = userId,
                Exception = message,
                PrimaryKeyValue = primaryKeyValue,
                MachineName = Environment.MachineName,
                Level = level.ToString()
            };
            await exceptionLoggerRepository.Insert(exceptionLog);
        }
        public async Task LogExceptionAsync(string message, long? userId = null, long? primaryKeyValue = null, string? level = null)
        {
            ExceptionLog exceptionLog = new()
            {
                CreatedDate = DateTime.Now,
                UserId = userId,
                Exception = message,
                PrimaryKeyValue = primaryKeyValue,
                MachineName = Environment.MachineName,
                Level = string.IsNullOrEmpty(level) ? "Info" : level
            };
            await exceptionLoggerRepository.Insert(exceptionLog);
        }
        public async Task WriteEntryAsync(string message, string? level = null)
        {
            ExceptionLog exceptionLog = new()
            {
                MachineName = Environment.MachineName,
                CreatedDate = DateTime.Now,
                Exception = message,
                Level = string.IsNullOrEmpty(level) ? "Info" : level
            };
            await exceptionLoggerRepository.Insert(exceptionLog);
        }

    }
}