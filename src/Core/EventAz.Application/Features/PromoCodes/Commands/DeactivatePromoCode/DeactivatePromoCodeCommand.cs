using MediatR;

namespace EventAz.Application.Features.PromoCodes.Commands.DeactivatePromoCode;

public sealed record DeactivatePromoCodeCommand(Guid Id) : IRequest<bool>;
