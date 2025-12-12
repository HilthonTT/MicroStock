namespace Infrastructure.Outbox;

internal sealed class OutboxOptions
{
    public const string ConfigurationSectionName = "Outbox";

    public int IntervalInSeconds { get; init; }

    public int BatchSize { get; init; }
}
