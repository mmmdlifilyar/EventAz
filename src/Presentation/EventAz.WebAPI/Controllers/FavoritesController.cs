using EventAz.Application.Features.Favorites.Commands.AddFavorite;
using EventAz.Application.Features.Favorites.Commands.RemoveFavorite;
using EventAz.Application.Features.Favorites.Queries.GetMyFavorites;
using EventAz.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventAz.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class FavoritesController : ControllerBase
{
    private readonly IMediator _mediator;
    public FavoritesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetMine()
        => Ok(await _mediator.Send(new GetMyFavoritesQuery()));

    [HttpPut("{eventId:guid}")]
    public async Task<IActionResult> Add(Guid eventId)
    {
        var result = await _mediator.Send(new AddFavoriteCommand(eventId));
        return result == FavoriteAddResult.EventUnavailable
            ? NotFound(new { error = "PublishedEventNotFound" })
            : NoContent();
    }

    [HttpDelete("{eventId:guid}")]
    public async Task<IActionResult> Remove(Guid eventId)
    {
        await _mediator.Send(new RemoveFavoriteCommand(eventId));
        return NoContent();
    }
}
