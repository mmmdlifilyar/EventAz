using MediatR;

namespace EventAz.Application.Features.Events.Queries.GetAdminEvents;

public sealed record GetAdminEventsQuery : IRequest<IReadOnlyList<EventDto>>;
