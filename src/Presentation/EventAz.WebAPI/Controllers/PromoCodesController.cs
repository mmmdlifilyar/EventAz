using EventAz.Application.Features.PromoCodes.Commands.CreatePromoCode;
using EventAz.Application.Features.PromoCodes.Commands.DeactivatePromoCode;
using EventAz.Application.Features.PromoCodes.Queries.GetPromoCodesByEvent;
using EventAz.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventAz.WebAPI.Controllers;

[ApiController]
[Route("api/promo-codes")]
[Authorize(Roles = "Admin")]
public sealed class PromoCodesController : ControllerBase
{
    private readonly IMediator _mediator;
    public PromoCodesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create(CreatePromoCodeCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.PromoCode is not null)
            return CreatedAtAction(nameof(GetByEvent), new { eventId = result.PromoCode.EventId }, result.PromoCode);

        return result.Error switch
        {
            PromoCodeCreateResult.DuplicateCode => Conflict(new { error = "PromoCodeAlreadyExists" }),
            PromoCodeCreateResult.EventNotFound => NotFound(new { error = "EventNotFound" }),
            _ => BadRequest(new { error = "PromoCodeCreationFailed" })
        };
    }

    [HttpGet("by-event/{eventId:guid}")]
    public async Task<IActionResult> GetByEvent(Guid eventId)
        => Ok(await _mediator.Send(new GetPromoCodesByEventQuery(eventId)));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
        => await _mediator.Send(new DeactivatePromoCodeCommand(id)) ? NoContent() : NotFound();
}
