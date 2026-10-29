using EventAz.Application.Features.Events.Commands.CreateEvent;
using EventAz.Application.Features.Events.Commands.PublishEvent;
using EventAz.Application.Features.Events.Queries.GetEventById;
using EventAz.Application.Features.Events.Queries.GetPublishedEvents;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventAz.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EventsController : ControllerBase
{
    private readonly IMediator _mediator;
    public EventsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublished([FromQuery] GetPublishedEventsQuery query)
        => Ok(await _mediator.Send(query));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetEventByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Organizer")]
    public async Task<IActionResult> Create(CreateEventCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Publish(Guid id)
    {
        await _mediator.Send(new PublishEventCommand(id));
        return NoContent();
    }
}
