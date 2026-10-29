namespace EventAz.Application.Features.Tickets;

public sealed record TicketDto(
    Guid Id,
    string QRCode,
    decimal PricePaid,
    DateTime PurchaseDate,
    string Status,
    Guid TicketTierId);
