using EventAz.Application.Features.Auth.Commands.Login;
using EventAz.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using EventAz.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using EventAz.Infrastructure.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace EventAz.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserManager<ApplicationUser> _users;
    private readonly SmtpSettings _email;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    public AuthController(IMediator mediator, UserManager<ApplicationUser> users, IOptions<SmtpSettings> email,
        IConfiguration configuration, ILogger<AuthController> logger)
        => (_mediator, _users, _email, _configuration, _logger) = (mediator, users, email.Value, configuration, logger);

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand command)
    {
        var token = await _mediator.Send(command);
        return Ok(new { token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        var token = await _mediator.Send(command);
        return Ok(new { token });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken ct)
    {
        const string response = "E-poçt sistemdə qeydiyyatdadırsa, şifrə sıfırlama linki göndərildi.";
        if (string.IsNullOrWhiteSpace(_email.Host) || string.IsNullOrWhiteSpace(_email.FromEmail) ||
            string.IsNullOrWhiteSpace(_email.UserName) || string.IsNullOrWhiteSpace(_email.Password))
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "Gmail göndərişi qurulmayıb. EmailSettings-də UserName, FromEmail və Google App Password dəyərlərini təhlükəsiz konfiqurasiyada təyin edin." });
        var user = await _users.FindByEmailAsync(request.Email);
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
            return Ok(new { message = response });

        var token = await _users.GeneratePasswordResetTokenAsync(user);
        var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
        var resetUrl = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(frontendUrl,
            new Dictionary<string, string?> { ["resetEmail"] = user.Email, ["resetToken"] = token });
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_email.FromName, _email.FromEmail));
        message.To.Add(MailboxAddress.Parse(user.Email));
        message.Subject = "EventAz şifrə sıfırlama";
        message.Body = new TextPart("plain") { Text = $"Şifrənizi yeniləmək üçün bu linki açın (30 dəqiqə ərzində):\n\n{resetUrl}\n\nƏgər bu sorğunu siz etməmisinizsə, bu e-poçtu nəzərə almayın." };
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(_email.TimeoutSeconds, 1, 60)));
            using var client = new SmtpClient();
            await client.ConnectAsync(_email.Host, _email.Port,
                _email.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls, timeout.Token);
            if (!string.IsNullOrWhiteSpace(_email.UserName))
                await client.AuthenticateAsync(_email.UserName, _email.Password, timeout.Token);
            await client.SendAsync(message, timeout.Token);
            await client.DisconnectAsync(true, timeout.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not send a password reset email.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "Sıfırlama e-poçtu göndərilmədi. SMTP parametrlərini və WebAPI jurnalını yoxlayın." });
        }
        return Ok(new { message = response });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var user = await _users.FindByEmailAsync(request.Email);
        if (user is null) return BadRequest(new { message = "Şifrə sıfırlama linki yanlışdır və ya vaxtı bitib." });
        var result = await _users.ResetPasswordAsync(user, request.Token, request.Password);
        if (!result.Succeeded) return BadRequest(new { message = string.Join(" ", result.Errors.Select(error => error.Description)) });
        return Ok(new { message = "Şifrəniz yeniləndi. İndi daxil ola bilərsiniz." });
    }
}

public sealed record ForgotPasswordRequest(string Email);
public sealed record ResetPasswordRequest(string Email, string Token, string Password);
