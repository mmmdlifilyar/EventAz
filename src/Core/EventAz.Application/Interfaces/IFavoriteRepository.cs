using EventAz.Domain.Entities;

namespace EventAz.Application.Interfaces;

public enum FavoriteAddResult
{
    Added,
    AlreadyExists,
    EventUnavailable
}

public interface IFavoriteRepository
{
    Task<FavoriteAddResult> AddAsync(string userId, Guid eventId, CancellationToken ct = default);
    Task RemoveAsync(string userId, Guid eventId, CancellationToken ct = default);
    Task<List<Event>> GetUserEventsAsync(string userId, CancellationToken ct = default);
}
