
namespace Idempotency.Logging;

public static partial class IdempotencyCleanupLogs
{

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Starting idempotency cleanup process.")]
    public static partial void LogServiceStarting(ILogger logger);


    [LoggerMessage(EventId = 2, Level = LogLevel.Information,
     Message = "Deleted {Count} expired idempotency records.")]
    public static partial void LogDeletedRecords(ILogger logger, int count);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error,
        Message = "Error occurred during idempotency cleanup at {Time}")]
     public static partial void LogCleanupError(ILogger logger, DateTime time, Exception ex);
}
