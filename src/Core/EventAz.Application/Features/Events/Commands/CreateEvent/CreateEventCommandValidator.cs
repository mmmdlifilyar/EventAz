using FluentValidation;

namespace EventAz.Application.Features.Events.Commands.CreateEvent;

public sealed class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.StartDate).GreaterThan(DateTime.UtcNow);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
        RuleFor(x => x.Tiers).NotNull().NotEmpty();
        RuleForEach(x => x.Tiers).ChildRules(tier =>
        {
            tier.RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
            tier.RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
            tier.RuleFor(x => x.TotalQuantity).GreaterThan(0);
        });
    }
}
