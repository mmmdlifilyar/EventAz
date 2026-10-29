namespace EventAz.Infrastructure.Services;

public sealed class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "EventAz";
    public bool UseSsl { get; set; }
    public int TimeoutSeconds { get; set; } = 15;
}
