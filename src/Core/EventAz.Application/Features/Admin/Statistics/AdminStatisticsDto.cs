namespace EventAz.Application.Features.Admin.Statistics;

public sealed record TopSellingEventDto(Guid EventId, string Title, int TicketsSold, decimal Revenue);

public sealed record AdminStatisticsDto(
    int TotalUsers,
    int TotalEvents,
    int PublishedEvents,
    int TicketsSold,
    decimal TotalRevenue,
    IReadOnlyList<TopSellingEventDto> TopSellingEvents);
