using EventAz.Domain.Entities;

namespace EventAz.Application.Interfaces;

public enum TicketValidationStatus
{
    Validated,
    NotFound,
    AlreadyUsed,
    Cancelled,
    EventUnavailable,
    EventNotStarted,
    EventEnded,
    Forbidden
}

public sealed record TicketValidationResult(
    TicketValidationStatus Status,
    Guid? TicketId = null,
    Guid? EventId = null,
    string? EventTitle = null);

public interface ITicketOperationsRepository
{
    Task<Ticket?> GetOwnedTicketAsync(Guid ticketId, string userId, CancellationToken ct = default);
    Task<List<Ticket>> GetMyTicketsAsync(string userId, CancellationToken ct = default);
    Task<TicketValidationResult> ValidateAndRedeemAsync(
        string qrCode, string userId, bool isAdmin, CancellationToken ct = default);
}
