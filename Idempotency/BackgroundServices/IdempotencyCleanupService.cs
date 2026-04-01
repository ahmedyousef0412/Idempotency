namespace Idempotency.BackgroundServices;

public class IdempotencyCleanupService
    (
       IServiceScopeFactory scopeFactory
      ,ILogger<IdempotencyCleanupService> logger
    ) 
    : BackgroundService
{

    private readonly TimeSpan _period = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Idempotency Cleanup Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_period, stoppingToken);
                await CleanupExpiredRecordsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during idempotency cleanup.");
            }
        }
    }

    private async Task CleanupExpiredRecordsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;

        var deletedCount = await dbContext.IdempotencyRecords
                                          .Where(r => r.ExpireAt <= now)
                                          .ExecuteDeleteAsync(cancellationToken);


                                         
        if (deletedCount > 0 && logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Deleted {Count} expired idempotency records.", deletedCount);
        }
    }
}
