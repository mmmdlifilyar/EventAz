using EventAz.Domain.Common;
using EventAz.Domain.Enums;

namespace EventAz.Domain.Entities;

public class Event : BaseEntity
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Location { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Draft;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = default!;

    public string OrganizerId { get; set; } = default!;   // ApplicationUser.Id (Identity)

    public ICollection<TicketTier> Tiers { get; set; } = new List<TicketTier>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Favorite> FavoritedBy { get; set; } = new List<Favorite>();

    public double AverageRating => Reviews.Count == 0 ? 0 : Reviews.Average(r => r.Rating);
}
