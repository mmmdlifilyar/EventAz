using EventAz.Application.Features.Reviews.Commands.CreateReview;
using EventAz.Application.Features.Reviews.Queries.GetEventReviews;
using EventAz.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventAz.WebAPI.Controllers;

[ApiController]
[Route("api/reviews")]
public sealed class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ReviewsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("event/{eventId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByEvent(Guid eventId)
        => Ok(await _mediator.Send(new GetEventReviewsQuery(eventId)));

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateReviewCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Review is not null)
            return CreatedAtAction(nameof(GetByEvent), new { eventId = command.EventId }, result.Review);

        return result.Error switch
        {
            ReviewCreateResult.EventNotFound => NotFound(new { error = "PublishedEventNotFound" }),
            ReviewCreateResult.EventNotFinished => Conflict(new { error = "EventNotFinished" }),
            ReviewCreateResult.TicketNotPurchased => StatusCode(StatusCodes.Status403Forbidden, new { error = "TicketRequired" }),
            ReviewCreateResult.AlreadyReviewed => Conflict(new { error = "AlreadyReviewed" }),
            _ => BadRequest(new { error = "ReviewCreationFailed" })
        };
    }
}
