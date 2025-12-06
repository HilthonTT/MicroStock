using SharedKernel;

namespace Application.Abstractions.Email;

public static class EmailErrors
{
    public static Error SmtpError(string message) => Error.Problem(
        "Email.SmtpError",
        $"STMP error: {message}");

    public static Error Failure(string message) => Error.Problem(
        "Email.Failure",
        $"Error sending email: {message}");
}
