using EventAz.Application.Interfaces;
using EventAz.Domain.Entities;
using EventAz.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventAz.Persistence.Repositories;

public sealed class TicketOperationsRepository : ITicketOperationsRepository
{
    private readonly ApplicationDbContext _context;
    public TicketOperationsRepository(ApplicationDbContext context) => _context = context;

    public Task<Ticket?> GetOwnedTicketAsync(Guid ticketId, string userId, CancellationToken ct = default)
        => _context.Tickets.AsNoTracking()
            .Include(t => t.TicketTier)
            .ThenInclude(tier => tier.Event)
            .FirstOrDefaultAsync(t => t.Id == ticketId && t.UserId == userId, ct);

    public Task<List<Ticket>> GetMyTicketsAsync(string userId, CancellationToken ct = default)
        => _context.Tickets.AsNoTracking()
            .Where(t => t.UserId == userId)
            .Include(t => t.TicketTier)
            .ThenInclude(tier => tier.Event)
            .OrderByDescending(t => t.PurchaseDate)
            .ToListAsync(ct);

    public async Task<TicketValidationResult> ValidateAndRedeemAsync(
        string qrCode, string userId, bool isAdmin, CancellationToken ct = default)
    {
        var ticket = await _context.Tickets.AsNoTracking()
            .Include(t => t.TicketTier)
            .ThenInclude(tier => tier.Event)
            .FirstOrDefaultAsync(t => t.QRCode == qrCode, ct);
        if (ticket is null)
            return new TicketValidationResult(TicketValidationStatus.NotFound);

        var eventEntity = ticket.TicketTier.Event;
        if (!isAdmin && eventEntity.OrganizerId != userId)
            return new TicketValidationResult(TicketValidationStatus.Forbidden);
        if (ticket.Status == TicketStatus.Cancelled)
            return new TicketValidationResult(TicketValidationStatus.Cancelled);
        if (ticket.Status == TicketStatus.Used)
            return new TicketValidationResult(TicketValidationStatus.AlreadyUsed, ticket.Id, eventEntity.Id, eventEntity.Title);
        if (eventEntity.Status != EventStatus.Published)
            return new TicketValidationResult(TicketValidationStatus.EventUnavailable, ticket.Id, eventEntity.Id, eventEntity.Title);

        var now = DateTime.UtcNow;
        if (now < eventEntity.StartDate)
            return new TicketValidationResult(TicketValidationStatus.EventNotStarted, ticket.Id, eventEntity.Id, eventEntity.Title);
        if (now > eventEntity.EndDate)
            return new TicketValidationResult(TicketValidationStatus.EventEnded, ticket.Id, eventEntity.Id, eventEntity.Title);

        // The conditional update means simultaneous scans cannot both redeem the same ticket.
        var redeemed = await _context.Tickets
            .Where(t => t.Id == ticket.Id && t.Status == TicketStatus.Valid &&
                        t.TicketTier.Event.Status == EventStatus.Published &&
                        t.TicketTier.Event.StartDate <= now && t.TicketTier.Event.EndDate >= now &&
                        (isAdmin || t.TicketTier.Event.OrganizerId == userId))
            .ExecuteUpdateAsync(update => update.SetProperty(t => t.Status, TicketStatus.Used), ct);

        if (redeemed == 0)
        {
            var latestStatus = await _context.Tickets.AsNoTracking()
                .Where(t => t.Id == ticket.Id)
                .Select(t => (TicketStatus?)t.Status)
                .FirstOrDefaultAsync(ct);
            return latestStatus switch
            {
                TicketStatus.Used => new TicketValidationResult(TicketValidationStatus.AlreadyUsed, ticket.Id, eventEntity.Id, eventEntity.Title),
                TicketStatus.Cancelled => new TicketValidationResult(TicketValidationStatus.Cancelled, ticket.Id, eventEntity.Id, eventEntity.Title),
                _ => new TicketValidationResult(TicketValidationStatus.EventUnavailable, ticket.Id, eventEntity.Id, eventEntity.Title)
            };
        }

        return new TicketValidationResult(TicketValidationStatus.Validated, ticket.Id, eventEntity.Id, eventEntity.Title);
    }
}
