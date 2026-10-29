using EventAz.Domain.Common;

namespace EventAz.Domain.Entities;

public class Favorite : BaseEntity
{
    public string UserId { get; set; } = default!;

    public Guid EventId { get; set; }
    public Event Event { get; set; } = default!;
}
