namespace EventAz.Application.Features.Events;

public sealed record TicketTierDto(Guid Id, string Name, decimal Price, int TotalQuantity, int Available);

public sealed record EventDto(
    Guid Id,
    string Title,
    string Description,
    string Location,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    Guid CategoryId,
    string CategoryName,
    IReadOnlyList<TicketTierDto> Tiers,
    double AverageRating);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
