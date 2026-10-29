using EventAz.Application.Interfaces;
using EventAz.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventAz.Persistence.Repositories;

public class EventRepository : RepositoryBase<Event>, IEventRepository
{
    public EventRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Event?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Tiers)
            .Include(e => e.Reviews)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<List<Event>> GetPublishedWithDetailsAsync(CancellationToken ct = default)
    => await _context.Events
        .Include(e => e.Category)
        .Include(e => e.Tiers)
        .Include(e => e.Reviews)
        .Where(e => e.Status == EventAz.Domain.Enums.EventStatus.Published)
        .OrderBy(e => e.StartDate)
        .ToListAsync(ct);

    public async Task<Event?> GetPublishedByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await _context.Events
            .AsNoTracking()
            .Include(e => e.Category)
            .Include(e => e.Tiers)
            .Include(e => e.Reviews)
            .FirstOrDefaultAsync(e => e.Id == id && e.Status == EventAz.Domain.Enums.EventStatus.Published, ct);

    public async Task<(List<Event> Events, int TotalCount)> GetPublishedPageAsync(
        int page, int pageSize, string? search, Guid? categoryId,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        var query = _context.Events.AsNoTracking()
            .Where(e => e.Status == EventAz.Domain.Enums.EventStatus.Published);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e => e.Title.Contains(search) || e.Description.Contains(search) || e.Location.Contains(search));
        if (categoryId.HasValue)
            query = query.Where(e => e.CategoryId == categoryId.Value);
        if (fromDate.HasValue)
            query = query.Where(e => e.StartDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(e => e.StartDate <= toDate.Value);

        var totalCount = await query.CountAsync(ct);
        var events = await query
            .OrderBy(e => e.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(e => e.Category)
            .Include(e => e.Tiers)
            .Include(e => e.Reviews)
            .ToListAsync(ct);
        return (events, totalCount);
    }
}
