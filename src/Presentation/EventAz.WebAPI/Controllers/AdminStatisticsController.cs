using EventAz.Application.Features.Admin.Statistics;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventAz.WebAPI.Controllers;

[ApiController]
[Route("api/admin/statistics")]
[Authorize(Roles = "Admin")]
public sealed class AdminStatisticsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminStatisticsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<AdminStatisticsDto>> Get(CancellationToken ct)
        => Ok(await _mediator.Send(new GetAdminStatisticsQuery(), ct));
}
