using System.ComponentModel.DataAnnotations.Schema;

namespace Idempotency.Domain.Entities;

public class IdempotencyRecord
{
    public Guid Id { get; set; }
    public string IdempotencyKey { get; set; }
    public string RequestHash { get; set; } 
    public string ResponseBody { get; set; } 
    public string ContentType { get; set; } = "application/json";
    public int StatusCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpireAt { get; set; }



    public IdempotencyStatus GetStatus(Guid attemptedId, string attemptedRequestHash)
    {
        if (Id == attemptedId) return IdempotencyStatus.New;


        if (RequestHash != attemptedRequestHash) return IdempotencyStatus.Mismatch;

        if (string.IsNullOrEmpty(ResponseBody)) return IdempotencyStatus.InProgress;


        return IdempotencyStatus.Completed;
    }
}

