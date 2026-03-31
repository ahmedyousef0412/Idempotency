
namespace Idempotency.Controllers;

[Route("api/[controller]")]  //https://localhost:7136/api/orders
[ApiController]
public class OrdersController(AppDbContext context) : ControllerBase
{

    private readonly AppDbContext _context = context;


    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order is null)
            return NotFound();


        return Ok(order);
    }

    [HttpPost]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        Thread.Sleep(5000); // Simulate some processing delay
        var order = new Order
        {
            Id = Guid.NewGuid(),
            ProductName = request.ProductName,
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

       
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id },order);
    }

}

