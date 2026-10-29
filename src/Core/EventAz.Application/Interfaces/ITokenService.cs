namespace EventAz.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(string userId, string email, IList<string> roles);
}
