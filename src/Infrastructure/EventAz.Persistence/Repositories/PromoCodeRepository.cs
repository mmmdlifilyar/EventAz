using EventAz.Application.Interfaces;
using EventAz.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventAz.Persistence.Repositories;

public sealed class PromoCodeRepository : IPromoCodeRepository
{
    private readonly ApplicationDbContext _context;
    public PromoCodeRepository(ApplicationDbContext context) => _context = context;

    public async Task<PromoCodeCreateResult> CreateAsync(PromoCode promoCode, CancellationToken ct = default)
    {
        if (!await _context.Events.AnyAsync(e => e.Id == promoCode.EventId, ct))
            return PromoCodeCreateResult.EventNotFound;
        if (await _context.PromoCodes.AnyAsync(p => p.Code == promoCode.Code, ct))
            return PromoCodeCreateResult.DuplicateCode;

        await _context.PromoCodes.AddAsync(promoCode, ct);
        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            _context.Entry(promoCode).State = EntityState.Detached;
            if (await _context.PromoCodes.AnyAsync(p => p.Code == promoCode.Code, ct))
                return PromoCodeCreateResult.DuplicateCode;
            throw;
        }

        return PromoCodeCreateResult.Created;
    }

    public async Task<List<PromoCode>> GetByEventAsync(Guid eventId, CancellationToken ct = default)
        => await _context.PromoCodes.AsNoTracking()
            .Where(p => p.EventId == eventId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

    public async Task<bool> DeactivateAsync(Guid promoCodeId, CancellationToken ct = default)
    {
        var promoCode = await _context.PromoCodes.FirstOrDefaultAsync(p => p.Id == promoCodeId, ct);
        if (promoCode is null)
            return false;
        if (!promoCode.IsActive)
            return true;

        promoCode.IsActive = false;
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
