using EventAz.Application.Features.Admin.Statistics;
using EventAz.Application.Interfaces;
using EventAz.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventAz.Persistence.Repositories;

public sealed class AdminStatisticsRepository : IAdminStatisticsRepository
{
    private readonly ApplicationDbContext _context;
    public AdminStatisticsRepository(ApplicationDbContext context) => _context = context;

    public async Task<AdminStatisticsDto> GetAsync(CancellationToken ct = default)
    {
        var totalUsers = await _context.Users.CountAsync(ct);
        var totalEvents = await _context.Events.CountAsync(ct);
        var publishedEvents = await _context.Events
            .CountAsync(e => e.Status == EventStatus.Published, ct);

        var soldTickets = _context.Tickets.AsNoTracking()
            .Where(t => t.Status != TicketStatus.Cancelled);
        var ticketsSold = await soldTickets.CountAsync(ct);
        var totalRevenue = await soldTickets.SumAsync(t => (decimal?)t.PricePaid, ct) ?? 0m;

        var topSellingEvents = await soldTickets
            .GroupBy(t => new { t.TicketTier.EventId, t.TicketTier.Event.Title })
            .Select(group => new TopSellingEventDto(
                group.Key.EventId,
                group.Key.Title,
                group.Count(),
                group.Sum(t => t.PricePaid)))
            .OrderByDescending(eventStats => eventStats.TicketsSold)
            .ThenBy(eventStats => eventStats.Title)
            .Take(5)
            .ToListAsync(ct);

        return new AdminStatisticsDto(
            totalUsers,
            totalEvents,
            publishedEvents,
            ticketsSold,
            totalRevenue,
            topSellingEvents);
    }
}
