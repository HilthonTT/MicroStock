namespace MicroStock.Common.Infrastructure.Inbox;

internal sealed class InboxMessageConsumer(Guid inboxMessageId, string name)
{
    public Guid InboxMessageId { get; init; } = inboxMessageId;
    public string Name { get; init; } = name;
}