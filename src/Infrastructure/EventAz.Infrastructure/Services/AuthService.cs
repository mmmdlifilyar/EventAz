using EventAz.Application.Interfaces;
using EventAz.Persistence.Identity;
using Microsoft.AspNetCore.Identity;

namespace EventAz.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthService(UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<(bool Succeeded, string? Token, IEnumerable<string> Errors)> RegisterAsync(
        string fullName, string email, string password, string role)
    {
        var user = new ApplicationUser { UserName = email, Email = email, FullName = fullName };
        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
            return (false, null, result.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, role);
        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user.Id, user.Email!, roles);

        return (true, token, Enumerable.Empty<string>());
    }

    public async Task<(bool Succeeded, string? Token)> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, password))
            return (false, null);

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user.Id, user.Email!, roles);
        return (true, token);
    }
}
