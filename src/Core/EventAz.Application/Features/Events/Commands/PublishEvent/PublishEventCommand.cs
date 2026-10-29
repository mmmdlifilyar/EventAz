using MediatR;

namespace EventAz.Application.Features.Events.Commands.PublishEvent;

public sealed record PublishEventCommand(Guid EventId) : IRequest;
