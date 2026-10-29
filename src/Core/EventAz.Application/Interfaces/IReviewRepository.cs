using EventAz.Domain.Entities;

namespace EventAz.Application.Interfaces;

public enum ReviewCreateResult
{
    Created,
    EventNotFound,
    EventNotFinished,
    TicketNotPurchased,
    AlreadyReviewed
}

public interface IReviewRepository
{
    Task<ReviewCreateResult> CreateAsync(Review review, CancellationToken ct = default);
    Task<List<Review>> GetByEventAsync(Guid eventId, CancellationToken ct = default);
}
