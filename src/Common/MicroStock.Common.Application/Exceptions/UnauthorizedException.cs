using MicroStock.Common.Domain;

namespace MicroStock.Common.Application.Exceptions;

public sealed class UnauthorizedException(
    string requestName,
    Error? error = default,
    Exception? innerException = default)
    : Exception("Unauthorized exception", innerException)
{
    public string RequestName { get; } = requestName;

    public Error? Error { get; } = error;
}
