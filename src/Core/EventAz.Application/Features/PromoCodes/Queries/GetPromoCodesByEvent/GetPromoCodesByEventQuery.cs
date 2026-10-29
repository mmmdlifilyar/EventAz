using EventAz.Application.Features.PromoCodes;
using MediatR;

namespace EventAz.Application.Features.PromoCodes.Queries.GetPromoCodesByEvent;

public sealed record GetPromoCodesByEventQuery(Guid EventId) : IRequest<IReadOnlyList<PromoCodeDto>>;
