namespace EventAz.Application.Interfaces;

public interface IAuthService
{
    Task<(bool Succeeded, string? Token, IEnumerable<string> Errors)> RegisterAsync(
        string fullName, string email, string password, string role);

    Task<(bool Succeeded, string? Token)> LoginAsync(string email, string password);
}
