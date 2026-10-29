using EventAz.Application.Interfaces;
using EventAz.Domain.Entities;
using EventAz.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventAz.Persistence.Repositories;

public sealed class TicketPurchaseRepository : ITicketPurchaseRepository
{
    private readonly ApplicationDbContext _context;
    public TicketPurchaseRepository(ApplicationDbContext context) => _context = context;

    public async Task<TicketPurchaseResult> PurchaseAsync(
        Guid ticketTierId, string userId, string qrCode, string? promoCode,
        CancellationToken ct = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        var now = DateTime.UtcNow;
        var tier = await _context.TicketTiers.AsNoTracking()
            .Include(t => t.Event)
            .FirstOrDefaultAsync(t => t.Id == ticketTierId, ct);
        if (tier is null || tier.Event.Status != EventStatus.Published || tier.Event.StartDate <= now)
            return TicketPurchaseResult.Failure(TicketPurchaseError.EventUnavailable);

        // Conditional SQL update makes the stock check and decrement one atomic operation.
        var reserved = await _context.TicketTiers
            .Where(t => t.Id == ticketTierId && t.SoldQuantity < t.TotalQuantity)
            .ExecuteUpdateAsync(update => update.SetProperty(t => t.SoldQuantity, t => t.SoldQuantity + 1), ct);
        if (reserved == 0)
            return TicketPurchaseResult.Failure(TicketPurchaseError.SoldOut);

        PromoCode? appliedPromo = null;
        var pricePaid = tier.Price;
        if (!string.IsNullOrWhiteSpace(promoCode))
        {
            var normalizedCode = promoCode.Trim().ToUpperInvariant();
            appliedPromo = await _context.PromoCodes.AsNoTracking()
                .FirstOrDefaultAsync(p => p.EventId == tier.EventId && p.Code == normalizedCode, ct);
            if (appliedPromo is null || !appliedPromo.IsActive || appliedPromo.ExpiryDate < now || appliedPromo.UsedCount >= appliedPromo.UsageLimit)
            {
                await transaction.RollbackAsync(ct);
                return TicketPurchaseResult.Failure(TicketPurchaseError.InvalidPromoCode);
            }

            var promoUsed = await _context.PromoCodes
                .Where(p => p.Id == appliedPromo.Id && p.IsActive && p.ExpiryDate >= now && p.UsedCount < p.UsageLimit)
                .ExecuteUpdateAsync(update => update.SetProperty(p => p.UsedCount, p => p.UsedCount + 1), ct);
            if (promoUsed == 0)
            {
                await transaction.RollbackAsync(ct);
                return TicketPurchaseResult.Failure(TicketPurchaseError.InvalidPromoCode);
            }

            pricePaid = Math.Round(tier.Price * (100m - appliedPromo.DiscountPercent) / 100m, 2,
                MidpointRounding.AwayFromZero);
        }

        var ticket = new Ticket
        {
            QRCode = qrCode,
            PricePaid = pricePaid,
            PurchaseDate = now,
            UserId = userId,
            TicketTierId = ticketTierId,
            PromoCodeId = appliedPromo?.Id
        };
        await _context.Tickets.AddAsync(ticket, ct);
        await _context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return TicketPurchaseResult.Success(ticket);
    }
}
