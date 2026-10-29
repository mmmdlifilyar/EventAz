using EventAz.Application.Interfaces;
using MediatR;

namespace EventAz.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, string>
    {
        private readonly IAuthService _authService;
        public RegisterCommandHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<string> Handle(RegisterCommand request, CancellationToken ct)
        {
            var result = await _authService.RegisterAsync(
                request.FullName, request.Email, request.Password, request.Role);

            if (!result.Succeeded)
                throw new ApplicationException(string.Join(", ", result.Errors));

            return result.Token!;
        }
    }
}
