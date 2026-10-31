namespace EventAz.Application.Features.Tickets;

public sealed record TicketHistoryDto(
    Guid Id,
    string EventTitle,
    string Location,
    DateTime EventStartDate,
    string TierName,
    decimal PricePaid,
    DateTime PurchaseDate,
    string Status);
