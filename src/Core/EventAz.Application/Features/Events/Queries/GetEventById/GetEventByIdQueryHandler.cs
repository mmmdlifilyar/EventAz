using EventAz.Application.Interfaces;
using MediatR;

namespace EventAz.Application.Features.Events.Queries.GetEventById;

public sealed class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, EventDto?>
{
    private readonly IEventRepository _events;
    public GetEventByIdQueryHandler(IEventRepository events) => _events = events;

    public async Task<EventDto?> Handle(GetEventByIdQuery request, CancellationToken ct)
    {
        var eventEntity = await _events.GetPublishedByIdWithDetailsAsync(request.Id, ct);
        return eventEntity is null ? null : EventMappings.ToDto(eventEntity);
    }
}
