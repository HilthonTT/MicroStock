namespace Infrastructure.Email;

internal sealed class EmailOptions
{
    public required string SmtpServer { get; init; }

    public required int SmtpPort { get; init; }

    public required string Username { get; init; }

    public required string Password { get; init; }

    public required string FromEmail { get; init; }

    public required string FromName { get; init; }

    public required bool EnableSsl { get; init; } = true;

    public required int TimeoutInMs { get; init; } = 30_000; // 30 seconds
}
