using MediatR;

namespace EventAz.Application.Features.Tickets.Queries.GetMyTickets;

public sealed record GetMyTicketsQuery : IRequest<IReadOnlyList<TicketHistoryDto>>;
