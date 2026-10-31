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
        DateTime? fromDate, DateTime? toDate, string? location,
        decimal? minPrice, decimal? maxPrice, string? sortBy, CancellationToken ct = default)
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
        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(e => e.Location.Contains(location));
        if (minPrice.HasValue && maxPrice.HasValue)
            query = query.Where(e => e.Tiers.Any(t => t.Price >= minPrice.Value && t.Price <= maxPrice.Value));
        else if (minPrice.HasValue)
            query = query.Where(e => e.Tiers.Any(t => t.Price >= minPrice.Value));
        else if (maxPrice.HasValue)
            query = query.Where(e => e.Tiers.Any(t => t.Price <= maxPrice.Value));

        var totalCount = await query.CountAsync(ct);
        var orderedQuery = (sortBy ?? "date").Trim().ToLowerInvariant() switch
        {
            "price_asc" => query.OrderBy(e => e.Tiers
                .OrderBy(t => t.Price).Select(t => (decimal?)t.Price).FirstOrDefault())
                .ThenBy(e => e.StartDate),
            "price_desc" => query.OrderByDescending(e => e.Tiers
                .OrderByDescending(t => t.Price).Select(t => (decimal?)t.Price).FirstOrDefault())
                .ThenBy(e => e.StartDate),
            "date_desc" => query.OrderByDescending(e => e.StartDate),
            _ => query.OrderBy(e => e.StartDate)
        };
        var events = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(e => e.Category)
            .Include(e => e.Tiers)
            .Include(e => e.Reviews)
            .ToListAsync(ct);
        return (events, totalCount);
    }
}
