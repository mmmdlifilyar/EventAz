using EventAz.Application.Interfaces;
using EventAz.Domain.Entities;
using MediatR;

namespace EventAz.Application.Features.Reviews.Commands.CreateReview;

public sealed class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, CreateReviewResponse>
{
    private readonly IReviewRepository _reviews;
    private readonly ICurrentUserService _currentUser;

    public CreateReviewCommandHandler(IReviewRepository reviews, ICurrentUserService currentUser)
        => (_reviews, _currentUser) = (reviews, currentUser);

    public async Task<CreateReviewResponse> Handle(CreateReviewCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("An authenticated user is required to write a review.");
        var review = new Review
        {
            EventId = request.EventId,
            UserId = userId,
            Rating = request.Rating,
            Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim()
        };

        var result = await _reviews.CreateAsync(review, ct);
        if (result != ReviewCreateResult.Created)
            return new CreateReviewResponse(null, result);

        return new CreateReviewResponse(
            new ReviewDto(review.Id, review.EventId, review.Rating, review.Comment, review.CreatedAt), null);
    }
}
