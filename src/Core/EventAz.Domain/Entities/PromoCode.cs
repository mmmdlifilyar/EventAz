using EventAz.Domain.Common;

namespace EventAz.Domain.Entities;

public class PromoCode : BaseEntity
{
    public string Code { get; set; } = default!;
    public decimal DiscountPercent { get; set; }   // məs. 10 = 10%
    public DateTime ExpiryDate { get; set; }
    public int UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid EventId { get; set; }
    public Event Event { get; set; } = default!;

    public bool IsValid => IsActive && UsedCount < UsageLimit && DateTime.UtcNow <= ExpiryDate;
}
