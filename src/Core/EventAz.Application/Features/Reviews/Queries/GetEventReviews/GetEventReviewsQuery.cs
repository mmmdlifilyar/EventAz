using MediatR;

namespace EventAz.Application.Features.Reviews.Queries.GetEventReviews;

public sealed record GetEventReviewsQuery(Guid EventId) : IRequest<IReadOnlyList<ReviewDto>>;
