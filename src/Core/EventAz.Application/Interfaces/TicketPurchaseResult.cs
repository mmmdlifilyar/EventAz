using EventAz.Domain.Entities;

namespace EventAz.Application.Interfaces;

public enum TicketPurchaseError
{
    None,
    EventUnavailable,
    SoldOut,
    InvalidPromoCode
}

public sealed record TicketPurchaseResult(Ticket? Ticket, TicketPurchaseError Error)
{
    public static TicketPurchaseResult Success(Ticket ticket) => new(ticket, TicketPurchaseError.None);
    public static TicketPurchaseResult Failure(TicketPurchaseError error) => new(null, error);
}
