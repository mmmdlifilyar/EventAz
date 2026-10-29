using EventAz.Application.Interfaces;
using MediatR;

namespace EventAz.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, string>
{
    private readonly IAuthService _authService;
    public LoginCommandHandler(IAuthService authService) => _authService = authService;

    public async Task<string> Handle(LoginCommand request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);
        if (!result.Succeeded)
            throw new UnauthorizedAccessException("Email və ya şifrə yanlışdır");

        return result.Token!;
    }
}
