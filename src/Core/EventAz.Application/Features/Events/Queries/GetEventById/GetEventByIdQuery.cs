using MediatR;

namespace EventAz.Application.Features.Events.Queries.GetEventById;

public sealed record GetEventByIdQuery(Guid Id) : IRequest<EventDto?>;
