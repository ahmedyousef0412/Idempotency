using Idempotency.Logging;

namespace Idempotency.BackgroundServices;

public partial class IdempotencyCleanupService
    (
       IServiceScopeFactory scopeFactory
      , ILogger<IdempotencyCleanupService> logger
    )
    : BackgroundService
{

    private readonly TimeSpan _period = TimeSpan.FromSeconds(10);
    private readonly ILogger<IdempotencyCleanupService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        IdempotencyCleanupLogs.LogServiceStarting(_logger);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredRecordsAsync(stoppingToken);

                await Task.Delay(_period, stoppingToken);
            }

            catch(OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            catch (Exception ex)
            {
                IdempotencyCleanupLogs.LogCleanupError(_logger, DateTime.UtcNow, ex);
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }

    private async Task CleanupExpiredRecordsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        
        var deletedCount = await dbContext.IdempotencyRecords
                                          .Where(r => r.ExpireAt <= DateTime.UtcNow)
                                          .ExecuteDeleteAsync(cancellationToken);


        if (deletedCount > 0)
        {
            IdempotencyCleanupLogs.LogDeletedRecords(logger, deletedCount);
        }

    }

   
}
