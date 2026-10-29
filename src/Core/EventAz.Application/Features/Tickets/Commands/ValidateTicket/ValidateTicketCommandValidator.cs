using FluentValidation;

namespace EventAz.Application.Features.Tickets.Commands.ValidateTicket;

public sealed class ValidateTicketCommandValidator : AbstractValidator<ValidateTicketCommand>
{
    public ValidateTicketCommandValidator()
    {
        RuleFor(x => x.QRCode).NotEmpty().Length(64).Matches("^[A-Fa-f0-9]{64}$");
    }
}
