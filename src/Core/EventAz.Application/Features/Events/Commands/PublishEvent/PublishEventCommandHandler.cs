using EventAz.Application.Common.Exceptions;
using EventAz.Application.Interfaces;
using EventAz.Domain.Enums;
using MediatR;

namespace EventAz.Application.Features.Events.Commands.PublishEvent;

public sealed class PublishEventCommandHandler : IRequestHandler<PublishEventCommand>
{
    private readonly IEventRepository _events;
    public PublishEventCommandHandler(IEventRepository events) => _events = events;

    public async Task Handle(PublishEventCommand request, CancellationToken ct)
    {
        var eventEntity = await _events.GetByIdAsync(request.EventId, ct)
            ?? throw new NotFoundException("Event", request.EventId);
        if (eventEntity.StartDate <= DateTime.UtcNow)
            throw new InvalidOperationException("A past event cannot be published.");
        eventEntity.Status = EventStatus.Published;
        await _events.UpdateAsync(eventEntity, ct);
    }
}
