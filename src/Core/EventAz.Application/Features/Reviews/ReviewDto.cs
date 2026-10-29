namespace EventAz.Application.Features.Reviews;

public sealed record ReviewDto(Guid Id, Guid EventId, int Rating, string? Comment, DateTime CreatedAt);
