namespace EventAz.Application.Interfaces;

public interface ITicketPurchaseEmailNotifier
{
    Task SendConfirmationAsync(string userId, Guid ticketId, CancellationToken ct = default);
}
