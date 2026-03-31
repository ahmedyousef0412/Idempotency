namespace Idempotency.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public string ProductName { get; set; }
    public DateTime CreatedAt { get; set; }
}
