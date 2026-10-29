using MediatR;

namespace EventAz.Application.Features.Events.Queries.GetPublishedEvents;

public sealed record GetPublishedEventsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    Guid? CategoryId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null) : IRequest<PagedResult<EventDto>>;
