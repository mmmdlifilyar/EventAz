using EventAz.Domain.Entities;

namespace EventAz.Application.Interfaces;

public interface IEventRepository : IRepository<Event>
{
    Task<Event?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default); 
    Task<List<Event>> GetPublishedWithDetailsAsync(CancellationToken ct = default);
    Task<List<Event>> GetAllWithDetailsAsync(CancellationToken ct = default);
    Task<Event?> GetPublishedByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<(List<Event> Events, int TotalCount)> GetPublishedPageAsync(
        int page, int pageSize, string? search, Guid? categoryId,
        DateTime? fromDate, DateTime? toDate, string? location,
        decimal? minPrice, decimal? maxPrice, string? sortBy, CancellationToken ct = default);
}
