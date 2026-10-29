using EventAz.Domain.Common;

namespace EventAz.Domain.Entities;

public class Review : BaseEntity
{
    public int Rating { get; set; }         // 1-5
    public string? Comment { get; set; }

    public string UserId { get; set; } = default!;

    public Guid EventId { get; set; }
    public Event Event { get; set; } = default!;
}
