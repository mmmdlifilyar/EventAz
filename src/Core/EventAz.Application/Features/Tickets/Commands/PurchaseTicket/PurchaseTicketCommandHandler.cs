using System.Security.Cryptography;
using EventAz.Application.Interfaces;
using MediatR;

namespace EventAz.Application.Features.Tickets.Commands.PurchaseTicket;

public sealed class PurchaseTicketCommandHandler : IRequestHandler<PurchaseTicketCommand, PurchaseTicketResponse>
{
    private readonly ITicketPurchaseRepository _purchases;
    private readonly ITicketPurchaseEmailNotifier _emailNotifier;
    private readonly ICurrentUserService _currentUser;

    public PurchaseTicketCommandHandler(
        ITicketPurchaseRepository purchases,
        ITicketPurchaseEmailNotifier emailNotifier,
        ICurrentUserService currentUser)
        => (_purchases, _emailNotifier, _currentUser) = (purchases, emailNotifier, currentUser);

    public async Task<PurchaseTicketResponse> Handle(PurchaseTicketCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("An authenticated user is required to purchase a ticket.");
        var qrCode = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var result = await _purchases.PurchaseAsync(
            request.TicketTierId, userId, qrCode, request.PromoCode?.Trim(), ct);

        if (result.Ticket is not { } ticket)
            return new PurchaseTicketResponse(null, result.Error);

        await _emailNotifier.SendConfirmationAsync(userId, ticket.Id, ct);

        return new PurchaseTicketResponse(new TicketDto(ticket.Id, ticket.QRCode, ticket.PricePaid,
            ticket.PurchaseDate, ticket.Status.ToString(), ticket.TicketTierId), null);
    }
}
