namespace SharedKernel;

public interface IDomainEvent
{
    Guid Id { get; }

    DateTime OccurredAtUtc { get; }
}