using EventAz.Application.Features.PromoCodes;
using EventAz.Application.Interfaces;
using MediatR;

namespace EventAz.Application.Features.PromoCodes.Commands.CreatePromoCode;

public sealed record CreatePromoCodeCommand(
    Guid EventId,
    string Code,
    decimal DiscountPercent,
    DateTime ExpiryDate,
    int UsageLimit) : IRequest<PromoCodeCreateResponse>;

public sealed record PromoCodeCreateResponse(PromoCodeDto? PromoCode, PromoCodeCreateResult? Error);
