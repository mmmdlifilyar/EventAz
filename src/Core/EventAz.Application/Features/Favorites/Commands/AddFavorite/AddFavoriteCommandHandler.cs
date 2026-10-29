using EventAz.Application.Interfaces;
using MediatR;

namespace EventAz.Application.Features.Favorites.Commands.AddFavorite;

public sealed class AddFavoriteCommandHandler : IRequestHandler<AddFavoriteCommand, FavoriteAddResult>
{
    private readonly IFavoriteRepository _favorites;
    private readonly ICurrentUserService _currentUser;

    public AddFavoriteCommandHandler(IFavoriteRepository favorites, ICurrentUserService currentUser)
        => (_favorites, _currentUser) = (favorites, currentUser);

    public Task<FavoriteAddResult> Handle(AddFavoriteCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("An authenticated user is required to manage favorites.");
        return _favorites.AddAsync(userId, request.EventId, ct);
    }
}
