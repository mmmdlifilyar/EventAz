using MediatR;

namespace EventAz.Application.Features.Events.Commands.CreateEvent;

public sealed record CreateEventCommand(
    string Title,
    string Description,
    string Location,
    DateTime StartDate,
    DateTime EndDate,
    Guid CategoryId,
    IReadOnlyList<CreateTicketTierRequest> Tiers) : IRequest<Guid>;

public sealed record CreateTicketTierRequest(string Name, decimal Price, int TotalQuantity);
