using EventAz.Application.Interfaces;
using MediatR;

namespace EventAz.Application.Features.Tickets.Queries.GetMyTickets;

public sealed class GetMyTicketsQueryHandler : IRequestHandler<GetMyTicketsQuery, IReadOnlyList<TicketHistoryDto>>
{
    private readonly ITicketOperationsRepository _tickets;
    private readonly ICurrentUserService _currentUser;

    public GetMyTicketsQueryHandler(ITicketOperationsRepository tickets, ICurrentUserService currentUser)
        => (_tickets, _currentUser) = (tickets, currentUser);

    public async Task<IReadOnlyList<TicketHistoryDto>> Handle(GetMyTicketsQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("An authenticated user is required to view tickets.");
        var tickets = await _tickets.GetMyTicketsAsync(userId, ct);
        return tickets.Select(ticket => new TicketHistoryDto(
            ticket.Id,
            ticket.TicketTier.Event.Title,
            ticket.TicketTier.Event.Location,
            ticket.TicketTier.Event.StartDate,
            ticket.TicketTier.Name,
            ticket.PricePaid,
            ticket.PurchaseDate,
            ticket.Status.ToString())).ToList();
    }
}
