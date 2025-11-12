using MicroStock.Common.Domain;

namespace MicroStock.Common.Application.Exceptions;

public sealed class MicroStockException(
    string requestName,
    Error? error = default,
    Exception? innerException = default)
    : Exception("Application exception", innerException)
{
    public string RequestName { get; } = requestName;

    public Error? Error { get; } = error;
}
