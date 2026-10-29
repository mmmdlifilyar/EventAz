using EventAz.Application.Common.Exceptions;
using EventAz.Application.Interfaces;
using EventAz.Domain.Entities;
using MediatR;

namespace EventAz.Application.Features.Events.Commands.CreateEvent;

public sealed class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Guid>
{
    private readonly IEventRepository _events;
    private readonly ICategoryRepository _categories;
    private readonly ICurrentUserService _currentUser;

    public CreateEventCommandHandler(IEventRepository events, ICategoryRepository categories, ICurrentUserService currentUser)
        => (_events, _categories, _currentUser) = (events, categories, currentUser);

    public async Task<Guid> Handle(CreateEventCommand request, CancellationToken ct)
    {
        if (await _categories.GetByIdAsync(request.CategoryId, ct) is null)
            throw new NotFoundException("Category", request.CategoryId);

        var eventEntity = new Event
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Location = request.Location.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CategoryId = request.CategoryId,
            OrganizerId = _currentUser.UserId!,
            Tiers = request.Tiers.Select(t => new TicketTier
            {
                Name = t.Name.Trim(),
                Price = t.Price,
                TotalQuantity = t.TotalQuantity
            }).ToList()
        };

        await _events.AddAsync(eventEntity, ct);
        return eventEntity.Id;
    }
}
