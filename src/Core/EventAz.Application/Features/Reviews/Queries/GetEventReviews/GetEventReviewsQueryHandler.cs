using EventAz.Application.Interfaces;
using MediatR;

namespace EventAz.Application.Features.Reviews.Queries.GetEventReviews;

public sealed class GetEventReviewsQueryHandler : IRequestHandler<GetEventReviewsQuery, IReadOnlyList<ReviewDto>>
{
    private readonly IReviewRepository _reviews;
    public GetEventReviewsQueryHandler(IReviewRepository reviews) => _reviews = reviews;

    public async Task<IReadOnlyList<ReviewDto>> Handle(GetEventReviewsQuery request, CancellationToken ct)
    {
        var reviews = await _reviews.GetByEventAsync(request.EventId, ct);
        return reviews.Select(r => new ReviewDto(r.Id, r.EventId, r.Rating, r.Comment, r.CreatedAt)).ToList();
    }
}
