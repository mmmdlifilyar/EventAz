namespace EventAz.Application.Interfaces;

public interface ITicketPurchaseRepository
{
    Task<TicketPurchaseResult> PurchaseAsync(
        Guid ticketTierId, string userId, string qrCode, string? promoCode,
        CancellationToken ct = default);
}
