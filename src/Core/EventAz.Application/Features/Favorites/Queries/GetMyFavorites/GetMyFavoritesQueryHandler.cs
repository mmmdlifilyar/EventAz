using EventAz.Application.Features.Events;
using EventAz.Application.Interfaces;
using MediatR;

namespace EventAz.Application.Features.Favorites.Queries.GetMyFavorites;

public sealed class GetMyFavoritesQueryHandler : IRequestHandler<GetMyFavoritesQuery, IReadOnlyList<EventDto>>
{
    private readonly IFavoriteRepository _favorites;
    private readonly ICurrentUserService _currentUser;

    public GetMyFavoritesQueryHandler(IFavoriteRepository favorites, ICurrentUserService currentUser)
        => (_favorites, _currentUser) = (favorites, currentUser);

    public async Task<IReadOnlyList<EventDto>> Handle(GetMyFavoritesQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("An authenticated user is required to view favorites.");
        var events = await _favorites.GetUserEventsAsync(userId, ct);
        return events.Select(EventMappings.ToDto).ToList();
    }
}
