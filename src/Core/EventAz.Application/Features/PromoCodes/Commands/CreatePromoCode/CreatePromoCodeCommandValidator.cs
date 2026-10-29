using FluentValidation;

namespace EventAz.Application.Features.PromoCodes.Commands.CreatePromoCode;

public sealed class CreatePromoCodeCommandValidator : AbstractValidator<CreatePromoCodeCommand>
{
    public CreatePromoCodeCommandValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Code may contain only letters, numbers, hyphens, and underscores.");
        RuleFor(x => x.DiscountPercent).InclusiveBetween(0.01m, 100m);
        RuleFor(x => x.ExpiryDate).GreaterThan(DateTime.UtcNow);
        RuleFor(x => x.UsageLimit).GreaterThan(0);
    }
}
