using EventAz.Application.Features.Tickets.Commands.PurchaseTicket;
using EventAz.Application.Features.Tickets.Commands.ValidateTicket;
using EventAz.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace EventAz.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TicketsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITicketOperationsRepository _tickets;
    private readonly ICurrentUserService _currentUser;

    public TicketsController(IMediator mediator, ITicketOperationsRepository tickets, ICurrentUserService currentUser)
        => (_mediator, _tickets, _currentUser) = (mediator, tickets, currentUser);

    [HttpPost]
    public async Task<IActionResult> Purchase(PurchaseTicketCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Ticket is not null)
            return Ok(result.Ticket);

        return result.Error switch
        {
            TicketPurchaseError.EventUnavailable => Conflict(new { error = "EventUnavailable" }),
            TicketPurchaseError.SoldOut => Conflict(new { error = "SoldOut" }),
            TicketPurchaseError.InvalidPromoCode => BadRequest(new { error = "InvalidPromoCode" }),
            _ => BadRequest(new { error = "PurchaseFailed" })
        };
    }

    [HttpGet("{ticketId:guid}/qr")]
    public async Task<IActionResult> GetQrCode(Guid ticketId, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized();

        var ticket = await _tickets.GetOwnedTicketAsync(ticketId, userId, ct);
        if (ticket is null)
            return NotFound();
        if (ticket.Status == EventAz.Domain.Enums.TicketStatus.Cancelled)
            return Conflict(new { error = "TicketCancelled" });

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(ticket.QRCode, QRCodeGenerator.ECCLevel.Q);
        using var png = new PngByteQRCode(data);
        return File(png.GetGraphic(10), "image/png", $"eventaz-ticket-{ticket.Id}.png");
    }

    [HttpPost("validate")]
    [Authorize(Roles = "Admin,Organizer")]
    public async Task<IActionResult> Validate([FromBody] ValidateTicketCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Status switch
        {
            TicketValidationStatus.Validated => Ok(result),
            TicketValidationStatus.NotFound => NotFound(new { error = "TicketNotFound" }),
            TicketValidationStatus.Forbidden => Forbid(),
            TicketValidationStatus.AlreadyUsed => Conflict(new { error = "TicketAlreadyUsed" }),
            TicketValidationStatus.Cancelled => Conflict(new { error = "TicketCancelled" }),
            TicketValidationStatus.EventNotStarted => Conflict(new { error = "EventNotStarted" }),
            TicketValidationStatus.EventEnded => Conflict(new { error = "EventEnded" }),
            _ => Conflict(new { error = "EventUnavailable" })
        };
    }
}
