using System.Data;
using EventAz.Application.Interfaces;
using EventAz.Domain.Entities;
using EventAz.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventAz.Persistence.Repositories;

public sealed class FavoriteRepository : IFavoriteRepository
{
    private readonly ApplicationDbContext _context;
    public FavoriteRepository(ApplicationDbContext context) => _context = context;

    public async Task<FavoriteAddResult> AddAsync(string userId, Guid eventId, CancellationToken ct = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var eventIsPublished = await _context.Events.AnyAsync(
            e => e.Id == eventId && e.Status == EventStatus.Published, ct);
        if (!eventIsPublished)
        {
            await transaction.RollbackAsync(ct);
            return FavoriteAddResult.EventUnavailable;
        }

        var exists = await _context.Favorites.AnyAsync(f => f.UserId == userId && f.EventId == eventId, ct);
        if (exists)
        {
            await transaction.CommitAsync(ct);
            return FavoriteAddResult.AlreadyExists;
        }

        await _context.Favorites.AddAsync(new Favorite { UserId = userId, EventId = eventId }, ct);
        await _context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return FavoriteAddResult.Added;
    }

    public async Task RemoveAsync(string userId, Guid eventId, CancellationToken ct = default)
    {
        await _context.Favorites
            .Where(f => f.UserId == userId && f.EventId == eventId)
            .ExecuteDeleteAsync(ct);
    }

    public async Task<List<Event>> GetUserEventsAsync(string userId, CancellationToken ct = default)
        => await _context.Favorites
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Include(f => f.Event)
                .ThenInclude(e => e.Category)
            .Include(f => f.Event)
                .ThenInclude(e => e.Tiers)
            .Include(f => f.Event)
                .ThenInclude(e => e.Reviews)
            .Select(f => f.Event)
            .ToListAsync(ct);
}
