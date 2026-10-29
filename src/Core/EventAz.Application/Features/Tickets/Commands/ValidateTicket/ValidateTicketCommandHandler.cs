using EventAz.Application.Interfaces;
using MediatR;

namespace EventAz.Application.Features.Tickets.Commands.ValidateTicket;

public sealed class ValidateTicketCommandHandler : IRequestHandler<ValidateTicketCommand, TicketValidationResponse>
{
    private readonly ITicketOperationsRepository _tickets;
    private readonly ICurrentUserService _currentUser;

    public ValidateTicketCommandHandler(ITicketOperationsRepository tickets, ICurrentUserService currentUser)
        => (_tickets, _currentUser) = (tickets, currentUser);

    public async Task<TicketValidationResponse> Handle(ValidateTicketCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("An authenticated user is required to validate a ticket.");
        var result = await _tickets.ValidateAndRedeemAsync(request.QRCode, userId, _currentUser.IsAdmin, ct);
        return new TicketValidationResponse(result.Status, result.TicketId, result.EventId, result.EventTitle);
    }
}
