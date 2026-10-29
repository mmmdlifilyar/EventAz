using EventAz.Application.Interfaces;
using MediatR;

namespace EventAz.Application.Features.Favorites.Commands.RemoveFavorite;

public sealed class RemoveFavoriteCommandHandler : IRequestHandler<RemoveFavoriteCommand>
{
    private readonly IFavoriteRepository _favorites;
    private readonly ICurrentUserService _currentUser;

    public RemoveFavoriteCommandHandler(IFavoriteRepository favorites, ICurrentUserService currentUser)
        => (_favorites, _currentUser) = (favorites, currentUser);

    public Task Handle(RemoveFavoriteCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("An authenticated user is required to manage favorites.");
        return _favorites.RemoveAsync(userId, request.EventId, ct);
    }
}
