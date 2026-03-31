
namespace Idempotency.Filters;

public class IdempotencyFilter(IIdempotencyService idempotencyService) : IAsyncActionFilter
{
    private readonly IIdempotencyService _idempotencyService = idempotencyService;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        
        var key = context.HttpContext.Request.Headers["X-Idempotency-Key"].FirstOrDefault();

        if (string.IsNullOrEmpty(key)) // Enforce the client to provide an idempotency key
        {
            context.Result = new BadRequestObjectResult(new
            {
                Error = "Missing Header",
                Message = "The 'X-Idempotency-Key' header is required for this operation."
            });

            return;
        }
       
        if (!Guid.TryParse(key, out _))
        {
            context.Result = new BadRequestObjectResult("X-Idempotency-Key must be a valid GUID.");
            return;
        }

        var requestDto = context.ActionArguments.Values.FirstOrDefault();
        var requestHash = ComputeHash.ComputeHashForRequest(requestDto);

        
        var checkResult = await _idempotencyService.CheckAndReserveAsync(key, requestHash);


        // TODO: The block of switch statement will be refactored using state pattern in the future to make it more maintainable and extensible.
        switch (checkResult.Status)
        {
            case IdempotencyStatus.InProgress:
                context.HttpContext.Response.Headers.Append("Retry-After", "5");
                context.Result = new ConflictObjectResult(new
                {
                    Message = "Request is currently being processed. Please retry in 5 seconds."
                });
                return;

            case IdempotencyStatus.Mismatch:
                context.Result = new BadRequestObjectResult("Idempotency key reused with a different payload.");
                return;

            case IdempotencyStatus.Completed:
                context.HttpContext.Response.Headers.Append("X-Idempotency-Cache", "HIT");
                context.Result = new ContentResult
                {
                    Content = checkResult.Record!.ResponseBody,
                    ContentType = checkResult.Record.ContentType,
                    StatusCode = checkResult.Record.StatusCode
                };
                return;

            case IdempotencyStatus.New:
            default:
               
                var executedContext = await next();

                
                if (executedContext.Result is ObjectResult objectResult)
                {
                    
                    var responseBody = JsonSerializer.Serialize(objectResult.Value);
                    int statusCode = objectResult.StatusCode ?? 201;

                   
                    context.HttpContext.Response.Headers.Append("X-Idempotency-Key", key);

                    await _idempotencyService.SaveResponseAsync(key, responseBody, statusCode);
                }
                break;
        }
    }
}