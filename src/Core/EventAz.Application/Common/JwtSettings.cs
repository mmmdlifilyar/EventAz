namespace EventAz.Application.Common;

public class JwtSettings   // "public" mütləq olmalıdır
{
    public string SecretKey { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public int ExpiryMinutes { get; set; }
}
