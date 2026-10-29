using EventAz.Domain.Entities;

namespace EventAz.Application.Interfaces;

public enum PromoCodeCreateResult
{
    Created,
    DuplicateCode,
    EventNotFound
}

public interface IPromoCodeRepository
{
    Task<PromoCodeCreateResult> CreateAsync(PromoCode promoCode, CancellationToken ct = default);
    Task<List<PromoCode>> GetByEventAsync(Guid eventId, CancellationToken ct = default);
    Task<bool> DeactivateAsync(Guid promoCodeId, CancellationToken ct = default);
}
