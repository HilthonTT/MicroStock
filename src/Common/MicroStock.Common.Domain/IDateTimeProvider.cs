namespace MicroStock.Common.Domain;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
