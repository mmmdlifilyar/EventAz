using MediatR;

namespace EventAz.Application.Features.Favorites.Commands.RemoveFavorite;

public sealed record RemoveFavoriteCommand(Guid EventId) : IRequest;
