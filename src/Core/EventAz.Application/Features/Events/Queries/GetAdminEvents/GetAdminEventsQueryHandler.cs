using EventAz.Application.Interfaces;
using MediatR;

namespace EventAz.Application.Features.Events.Queries.GetAdminEvents;

public sealed class GetAdminEventsQueryHandler : IRequestHandler<GetAdminEventsQuery, IReadOnlyList<EventDto>>
{
    private readonly IEventRepository _events;

    public GetAdminEventsQueryHandler(IEventRepository events) => _events = events;

    public async Task<IReadOnlyList<EventDto>> Handle(GetAdminEventsQuery request, CancellationToken ct)
    {
        var events = await _events.GetAllWithDetailsAsync(ct);
        return events.Select(EventMappings.ToDto).ToList();
    }
}
