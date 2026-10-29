using EventAz.Application.Interfaces;
using MediatR;

namespace EventAz.Application.Features.PromoCodes.Commands.DeactivatePromoCode;

public sealed class DeactivatePromoCodeCommandHandler : IRequestHandler<DeactivatePromoCodeCommand, bool>
{
    private readonly IPromoCodeRepository _promoCodes;
    public DeactivatePromoCodeCommandHandler(IPromoCodeRepository promoCodes) => _promoCodes = promoCodes;

    public Task<bool> Handle(DeactivatePromoCodeCommand request, CancellationToken ct)
        => _promoCodes.DeactivateAsync(request.Id, ct);
}
