using EventAz.Domain.Common;

namespace EventAz.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = default!;

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
