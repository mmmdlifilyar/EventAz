using EventAz.Application.Features.Events;
using MediatR;

namespace EventAz.Application.Features.Favorites.Queries.GetMyFavorites;

public sealed record GetMyFavoritesQuery : IRequest<IReadOnlyList<EventDto>>;
