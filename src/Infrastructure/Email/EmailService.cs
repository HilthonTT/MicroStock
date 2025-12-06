using Application.Abstractions.Email;
using Microsoft.Extensions.Options;
using SharedKernel;
using System.Net;
using System.Net.Mail;

namespace Infrastructure.Email;

internal sealed class EmailService : IEmailService
{
    private readonly EmailOptions _options;

    public EmailService(IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }

    public async Task<List<Result>> SendBulkEmailAsync(List<EmailRequest> requests, CancellationToken cancellationToken = default)
    {
        if (requests.Count == 0)
        {
            return [];
        }

        IEnumerable<Task<Result>> tasks = requests.Select(r => SendEmailAsync(r, cancellationToken));
        Result[] results = await Task.WhenAll(tasks);

        return results.ToList();
    }

    public async Task<Result> SendEmailAsync(EmailRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            using MailMessage mailMessage = CreateMailMessage(request);
            using SmtpClient smtpClient = CreateSmtpClient();

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);
            return Result.Success();
        }
        catch (SmtpException ex)
        {
           return Result.Failure(EmailErrors.SmtpError(ex.Message));
        }
        catch (Exception ex)
        {
            return Result.Failure(EmailErrors.Failure(ex.Message));
        }
    }

    public Task<Result> SendEmailAsync(
        string to, 
        string subject, 
        string body, 
        bool isHtml = true,
        CancellationToken cancellationToken = default)
    {
        var request = new EmailRequest
        {
            To = [to],
            Subject = subject,
            Body = body,
            IsHtml = isHtml
        };

        return SendEmailAsync(request, cancellationToken);
    }

    private MailMessage CreateMailMessage(EmailRequest request)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, _options.FromName ?? _options.FromEmail),
            Subject = request.Subject,
            Body = request.Body,
            IsBodyHtml = request.IsHtml
        };

        // Add recipients
        foreach (string to in request.To)
        {
            mailMessage.To.Add(new MailAddress(to));
        }

        foreach (string cc in request.Cc)
        {
            mailMessage.CC.Add(new MailAddress(cc));
        }

        foreach (string bcc in request.Bcc)
        {
            mailMessage.Bcc.Add(new MailAddress(bcc));
        }

        foreach (EmailAttachment attachment in request.Attachments)
        {
            var stream = new MemoryStream(attachment.Content);
            var mailAttachment = new Attachment(stream, attachment.FileName, attachment.ContentType);
            mailMessage.Attachments.Add(mailAttachment);
        }

        foreach (KeyValuePair<string, string> header in request.Headers)
        {
            mailMessage.Headers.Add(header.Key, header.Value);
        }

        return mailMessage;
    }

    private SmtpClient CreateSmtpClient()
    {
        var smtpClient = new SmtpClient(_options.SmtpServer, _options.SmtpPort)
        {
            EnableSsl = _options.EnableSsl,
            Timeout = _options.TimeoutInMs,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            smtpClient.Credentials = new NetworkCredential(_options.Username, _options.Password);
        }

        return smtpClient;
    }
}
