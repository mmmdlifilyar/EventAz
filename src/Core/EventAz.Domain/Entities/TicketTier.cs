using EventAz.Domain.Common;

namespace EventAz.Domain.Entities;

public class TicketTier : BaseEntity
{
    public string Name { get; set; } = default!;   // "VIP", "Standard", "Economy"
    public decimal Price { get; set; }
    public int TotalQuantity { get; set; }
    public int SoldQuantity { get; set; }

    public Guid EventId { get; set; }
    public Event Event { get; set; } = default!;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public int Available => TotalQuantity - SoldQuantity;
}
