using SharedKernel;
using System.Net.Mail;

namespace Application.Abstractions.Email;

public interface IEmailService
{
    Task<Result> SendEmailAsync(EmailRequest request, CancellationToken cancellationToken = default);

    Task<Result> SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);

    Task<List<Result>> SendBulkEmailAsync(List<EmailRequest> requests, CancellationToken cancellationToken = default);
}
