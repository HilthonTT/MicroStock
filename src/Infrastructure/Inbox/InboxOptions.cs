namespace Infrastructure.Inbox;

internal sealed class InboxOptions 
{
    public const string ConfigurationSectionName = "Inbox";

    public int IntervalInSeconds { get; init; }

    public int BatchSize { get; init; }
}
