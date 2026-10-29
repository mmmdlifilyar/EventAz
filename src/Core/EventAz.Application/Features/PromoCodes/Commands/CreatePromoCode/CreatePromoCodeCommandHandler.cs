using EventAz.Application.Interfaces;
using EventAz.Domain.Entities;
using MediatR;

namespace EventAz.Application.Features.PromoCodes.Commands.CreatePromoCode;

public sealed class CreatePromoCodeCommandHandler : IRequestHandler<CreatePromoCodeCommand, PromoCodeCreateResponse>
{
    private readonly IPromoCodeRepository _promoCodes;
    public CreatePromoCodeCommandHandler(IPromoCodeRepository promoCodes) => _promoCodes = promoCodes;

    public async Task<PromoCodeCreateResponse> Handle(CreatePromoCodeCommand request, CancellationToken ct)
    {
        var promoCode = new PromoCode
        {
            EventId = request.EventId,
            Code = request.Code.Trim().ToUpperInvariant(),
            DiscountPercent = request.DiscountPercent,
            ExpiryDate = request.ExpiryDate,
            UsageLimit = request.UsageLimit,
            IsActive = true
        };

        var result = await _promoCodes.CreateAsync(promoCode, ct);
        if (result != PromoCodeCreateResult.Created)
            return new PromoCodeCreateResponse(null, result);

        return new PromoCodeCreateResponse(ToDto(promoCode), null);
    }

    private static PromoCodeDto ToDto(PromoCode promoCode) => new(
        promoCode.Id, promoCode.EventId, promoCode.Code, promoCode.DiscountPercent,
        promoCode.ExpiryDate, promoCode.UsageLimit, promoCode.UsedCount, promoCode.IsActive);
}
