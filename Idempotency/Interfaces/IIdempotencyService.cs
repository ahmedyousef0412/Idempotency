
namespace Idempotency.Interfaces;

public interface IIdempotencyService
{

    Task<IdempotencyRecord?> GetAsync(string key);
    Task SaveResponseAsync(string key, string response, int statusCode);
    Task<IdempotencyCheckResult> CheckAndReserveAsync(string key, string requestHash);
}

public class IdempotencyCheckResult
{
    public IdempotencyStatus Status { get; set; }
    public IdempotencyRecord? Record { get; set; }
}
public enum IdempotencyStatus
{
    New,          // Brand new request, proceed to Controller
    InProgress,   // Another thread is working on this right now
    Completed,    // Work is done, return the cached Record
    Mismatch      // Key exists but RequestHash is different (Potential Fraud/Bug)
}