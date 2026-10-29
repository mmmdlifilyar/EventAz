using EventAz.Domain.Common;
using EventAz.Domain.Enums;

namespace EventAz.Domain.Entities;

public class Ticket : BaseEntity
{
    public string QRCode { get; set; } = default!;
    public decimal PricePaid { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    public TicketStatus Status { get; set; } = TicketStatus.Valid;

    public string UserId { get; set; } = default!;

    public Guid TicketTierId { get; set; }
    public TicketTier TicketTier { get; set; } = default!;

    public Guid? PromoCodeId { get; set; }
    public PromoCode? PromoCode { get; set; }
}
