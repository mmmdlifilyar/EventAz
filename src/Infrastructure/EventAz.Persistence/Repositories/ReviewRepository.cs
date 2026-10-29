using System.Data;
using EventAz.Application.Interfaces;
using EventAz.Domain.Entities;
using EventAz.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventAz.Persistence.Repositories;

public sealed class ReviewRepository : IReviewRepository
{
    private readonly ApplicationDbContext _context;
    public ReviewRepository(ApplicationDbContext context) => _context = context;

    public async Task<ReviewCreateResult> CreateAsync(Review review, CancellationToken ct = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var eventEntity = await _context.Events.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == review.EventId && e.Status == EventStatus.Published, ct);
        if (eventEntity is null)
            return ReviewCreateResult.EventNotFound;
        if (eventEntity.EndDate > DateTime.UtcNow)
            return ReviewCreateResult.EventNotFinished;

        var hasValidTicket = await _context.Tickets.AnyAsync(t =>
            t.UserId == review.UserId &&
            t.Status != TicketStatus.Cancelled &&
            t.TicketTier.EventId == review.EventId, ct);
        if (!hasValidTicket)
            return ReviewCreateResult.TicketNotPurchased;

        if (await _context.Reviews.AnyAsync(r => r.UserId == review.UserId && r.EventId == review.EventId, ct))
            return ReviewCreateResult.AlreadyReviewed;

        await _context.Reviews.AddAsync(review, ct);
        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(ct);
            _context.Entry(review).State = EntityState.Detached;
            if (await _context.Reviews.AnyAsync(r => r.UserId == review.UserId && r.EventId == review.EventId, ct))
                return ReviewCreateResult.AlreadyReviewed;
            throw;
        }

        await transaction.CommitAsync(ct);
        return ReviewCreateResult.Created;
    }

    public async Task<List<Review>> GetByEventAsync(Guid eventId, CancellationToken ct = default)
        => await _context.Reviews.AsNoTracking()
            .Where(r => r.EventId == eventId && r.Event.Status == EventStatus.Published)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
}
