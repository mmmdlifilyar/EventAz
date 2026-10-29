using EventAz.Application.Features.Tickets;
using EventAz.Application.Interfaces;
using MediatR;

namespace EventAz.Application.Features.Tickets.Commands.PurchaseTicket;

public sealed record PurchaseTicketResponse(TicketDto? Ticket, TicketPurchaseError? Error);

public sealed record PurchaseTicketCommand(Guid TicketTierId, string? PromoCode = null) : IRequest<PurchaseTicketResponse>;
