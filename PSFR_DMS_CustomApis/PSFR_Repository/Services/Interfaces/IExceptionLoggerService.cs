using PSFR_Repository.Enums;

namespace PSFR_Repository.Services.Interfaces
{
    public interface IExceptionLoggerService
    {
        Task LogExceptionAsync(Exception ex, long? userId = null, long? primaryKeyValue = null, LoggingLevel level = LoggingLevel.Error);
        Task LogExceptionAsync(string message, long? userId = null, long? primaryKeyValue = null, LoggingLevel level = LoggingLevel.Error);
        Task LogExceptionAsync(string message, long? userId = null, long? primaryKeyValue = null, string? level = null);
        Task WriteEntryAsync(string message, string? level = null);
    }
}
