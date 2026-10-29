using FluentValidation;

namespace EventAz.Application.Features.Tickets.Commands.PurchaseTicket;

public sealed class PurchaseTicketCommandValidator : AbstractValidator<PurchaseTicketCommand>
{
    public PurchaseTicketCommandValidator()
    {
        RuleFor(x => x.TicketTierId).NotEmpty();
        RuleFor(x => x.PromoCode).MaximumLength(50);
    }
}
