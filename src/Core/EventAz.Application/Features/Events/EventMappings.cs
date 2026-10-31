using EventAz.Domain.Entities;

namespace EventAz.Application.Features.Events;

internal static class EventMappings
{
    public static EventDto ToDto(Event eventEntity) => new(
        eventEntity.Id,
        eventEntity.Title,
        eventEntity.Description,
        eventEntity.Location,
        eventEntity.StartDate,
        eventEntity.EndDate,
        eventEntity.Status.ToString(),
        eventEntity.CategoryId,
        eventEntity.Category.Name,
        eventEntity.Tiers.Select(t => new TicketTierDto(t.Id, t.Name, t.Price, t.TotalQuantity, t.Available)).ToList(),
        eventEntity.AverageRating);
}
