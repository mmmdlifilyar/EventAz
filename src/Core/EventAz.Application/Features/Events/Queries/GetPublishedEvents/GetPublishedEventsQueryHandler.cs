using EventAz.Application.Interfaces;
using MediatR;

namespace EventAz.Application.Features.Events.Queries.GetPublishedEvents;

public sealed class GetPublishedEventsQueryHandler : IRequestHandler<GetPublishedEventsQuery, PagedResult<EventDto>>
{
    private readonly IEventRepository _events;
    public GetPublishedEventsQueryHandler(IEventRepository events) => _events = events;

    public async Task<PagedResult<EventDto>> Handle(GetPublishedEventsQuery request, CancellationToken ct)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var (events, totalCount) = await _events.GetPublishedPageAsync(
            page, pageSize, request.Search?.Trim(), request.CategoryId, request.FromDate, request.ToDate,
            request.Location?.Trim(), request.MinPrice, request.MaxPrice, request.SortBy, ct);
        return new PagedResult<EventDto>(events.Select(EventMappings.ToDto).ToList(), page, pageSize, totalCount);
    }
}
