using MediatR;
using EventAz.Application.Interfaces;

namespace EventAz.Application.Features.Favorites.Commands.AddFavorite;

public sealed record AddFavoriteCommand(Guid EventId) : IRequest<FavoriteAddResult>;
