using MediatR;
using EventAz.Application.Interfaces;

namespace EventAz.Application.Features.Reviews.Commands.CreateReview;

public sealed record CreateReviewCommand(Guid EventId, int Rating, string? Comment) : IRequest<CreateReviewResponse>;

public sealed record CreateReviewResponse(ReviewDto? Review, ReviewCreateResult? Error);
