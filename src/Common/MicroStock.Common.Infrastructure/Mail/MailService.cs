using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MicroStock.Common.Application.Mail;
using MimeKit;

namespace MicroStock.Common.Infrastructure.Mail;

internal sealed class MailService : IMailService
{
    private readonly MailOptions _options;
    private readonly ILogger<MailService> _logger;

    public MailService(IOptions<MailOptions> settings, ILogger<MailService> logger)
    {
        _options = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(MailRequest request, CancellationToken cancellationToken = default)
    {
        using MimeMessage email = BuildMimeMessage(request);

        await SendViaSmtpAsync(email, cancellationToken);
    }

    private MimeMessage BuildMimeMessage(MailRequest request)
    {
        using var email = new MimeMessage();

        // From & Sender
        string? fromAddress = request.From ?? _options.From;
        string? displayName = request.DisplayName ?? _options.DisplayName;
        var sender = new MailboxAddress(displayName, fromAddress);
        email.From.Add(sender);
        email.Sender = sender;

        // To
        foreach (var address in request.To ?? Enumerable.Empty<string>())
        {
            if (!string.IsNullOrWhiteSpace(address))
            {
                email.To.Add(MailboxAddress.Parse(address.Trim()));
            }
        }
        

        // ReplyTo
        if (!string.IsNullOrWhiteSpace(request.ReplyTo))
        {
            email.ReplyTo.Add(new MailboxAddress(request.ReplyToName, request.ReplyTo));
        }
        

        // Cc
        if (request.Cc != null)
        {
            foreach (var address in request.Cc)
            {
                if (!string.IsNullOrWhiteSpace(address))
                {
                    email.Cc.Add(MailboxAddress.Parse(address.Trim()));
                }
            }
        }
        
        // Bcc
        if (request.Bcc != null)
        {
            foreach (var address in request.Bcc)
            {
                if (!string.IsNullOrWhiteSpace(address))
                {
                    email.Bcc.Add(MailboxAddress.Parse(address.Trim()));
                }
            }
        }

        // Headers
        if (request.Headers != null)
        {
            foreach (var header in request.Headers)
            {
                email.Headers.Add(header.Key, header.Value);
            }
        }

        // Subject & Body
        email.Subject = request.Subject ?? string.Empty;
        var builder = new BodyBuilder
        {
            HtmlBody = request.Body
        };

        // Attachments
        if (request.AttachmentData != null)
        {
            foreach (KeyValuePair<string, byte[]> attachment in request.AttachmentData)
            {
                if (attachment.Value == null || attachment.Value.Length == 0)
                {
                    continue;
                }

                using var stream = new MemoryStream();
                stream.Write(attachment.Value, 0, attachment.Value.Length);
                stream.Position = 0;
                builder.Attachments.Add(attachment.Key, stream, ContentType.Parse("application/octet-stream"));
            }
        }

        email.Body = builder.ToMessageBody();
        return email;
    }

    private async Task SendViaSmtpAsync(MimeMessage email, CancellationToken cancellationToken)
    {
        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(_options.UserName, _options.Password, cancellationToken);
            await client.SendAsync(email, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while sending email: {Message}", ex.Message);
            throw; // Re-throw to allow caller to handle failure
        }
        finally
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync(true, cancellationToken);
            }
        }
    }
}
