using EventAz.Application.Interfaces;
using MediatR;

namespace EventAz.Application.Features.PromoCodes.Queries.GetPromoCodesByEvent;

public sealed class GetPromoCodesByEventQueryHandler : IRequestHandler<GetPromoCodesByEventQuery, IReadOnlyList<PromoCodeDto>>
{
    private readonly IPromoCodeRepository _promoCodes;
    public GetPromoCodesByEventQueryHandler(IPromoCodeRepository promoCodes) => _promoCodes = promoCodes;

    public async Task<IReadOnlyList<PromoCodeDto>> Handle(GetPromoCodesByEventQuery request, CancellationToken ct)
    {
        var promoCodes = await _promoCodes.GetByEventAsync(request.EventId, ct);
        return promoCodes.Select(p => new PromoCodeDto(
            p.Id, p.EventId, p.Code, p.DiscountPercent, p.ExpiryDate, p.UsageLimit, p.UsedCount, p.IsActive)).ToList();
    }
}
