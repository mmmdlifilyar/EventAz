using EventAz.Application.Interfaces;
using EventAz.Persistence;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using QRCoder;

namespace EventAz.Infrastructure.Services;

public sealed class TicketPurchaseEmailNotifier : ITicketPurchaseEmailNotifier
{
    private readonly ApplicationDbContext _context;
    private readonly SmtpSettings _settings;
    private readonly ILogger<TicketPurchaseEmailNotifier> _logger;

    public TicketPurchaseEmailNotifier(
        ApplicationDbContext context,
        IOptions<SmtpSettings> settings,
        ILogger<TicketPurchaseEmailNotifier> logger)
        => (_context, _settings, _logger) = (context, settings.Value, logger);

    public async Task SendConfirmationAsync(string userId, Guid ticketId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            _logger.LogInformation("Ticket confirmation email was skipped because SMTP is not configured.");
            return;
        }

        var recipient = await _context.Users.AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new { user.Email, user.FullName })
            .FirstOrDefaultAsync(ct);
        var ticket = await _context.Tickets.AsNoTracking()
            .Include(item => item.TicketTier)
            .ThenInclude(tier => tier.Event)
            .FirstOrDefaultAsync(item => item.Id == ticketId && item.UserId == userId, ct);

        if (string.IsNullOrWhiteSpace(recipient?.Email) || ticket is null)
        {
            _logger.LogWarning("Ticket confirmation email could not be prepared for ticket {TicketId}.", ticketId);
            return;
        }

        var eventEntity = ticket.TicketTier.Event;
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(new MailboxAddress(recipient.FullName ?? recipient.Email, recipient.Email));
        message.Subject = $"EventAz ticket confirmation: {eventEntity.Title}";
        var body = new BodyBuilder
        {
            TextBody = $"Hello {recipient.FullName ?? recipient.Email},\n\n" +
                   $"Your ticket purchase is confirmed.\n\n" +
                   $"Event: {eventEntity.Title}\n" +
                   $"Location: {eventEntity.Location}\n" +
                   $"Starts: {eventEntity.StartDate:u}\n" +
                   $"Ticket tier: {ticket.TicketTier.Name}\n" +
                   $"Price paid: {ticket.PricePaid:0.00}\n" +
                   $"Ticket ID: {ticket.Id}\n\n" +
                   "Keep this email available when you attend."
        };
        using (var generator = new QRCodeGenerator())
        using (var qrData = generator.CreateQrCode(ticket.QRCode, QRCodeGenerator.ECCLevel.Q))
        using (var qrPng = new PngByteQRCode(qrData))
            body.Attachments.Add($"eventaz-ticket-{ticket.Id}.png", qrPng.GetGraphic(10), new ContentType("image", "png"));
        message.Body = body.ToMessageBody();

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(_settings.TimeoutSeconds, 1, 60)));
        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                _settings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls,
                timeout.Token);

            if (!string.IsNullOrWhiteSpace(_settings.UserName))
                await client.AuthenticateAsync(_settings.UserName, _settings.Password, timeout.Token);

            await client.SendAsync(message, timeout.Token);
            await client.DisconnectAsync(true, timeout.Token);
            _logger.LogInformation("Ticket confirmation email sent for ticket {TicketId}.", ticketId);
        }
        catch (Exception exception)
        {
            // Email is best effort after the ticket is committed; a mail outage must not turn a successful purchase into an API failure.
            _logger.LogError(exception, "Could not send ticket confirmation email for ticket {TicketId}.", ticketId);
        }
    }
}
