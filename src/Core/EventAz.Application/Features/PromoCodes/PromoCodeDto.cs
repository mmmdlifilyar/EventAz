namespace EventAz.Application.Features.PromoCodes;

public sealed record PromoCodeDto(
    Guid Id,
    Guid EventId,
    string Code,
    decimal DiscountPercent,
    DateTime ExpiryDate,
    int UsageLimit,
    int UsedCount,
    bool IsActive);
