using EventAz.Application.Interfaces;
using MediatR;

namespace EventAz.Application.Features.Tickets.Commands.ValidateTicket;

public sealed record ValidateTicketCommand(string QRCode) : IRequest<TicketValidationResponse>;

public sealed record TicketValidationResponse(
    TicketValidationStatus Status,
    Guid? TicketId,
    Guid? EventId,
    string? EventTitle);
