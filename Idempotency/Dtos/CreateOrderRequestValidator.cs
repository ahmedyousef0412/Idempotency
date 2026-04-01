using FluentValidation;

namespace Idempotency.Dtos;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product Name is required.")
            .MinimumLength(3).WithMessage("Product Name must be at least 3 characters.")
            .MaximumLength(100).WithMessage("Product Name cannot exceed 100 characters.");


        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.")
            .LessThanOrEqualTo(5).WithMessage("Quantity cannot exceed 5.");

    }
}
