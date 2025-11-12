using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroStock.Common.Infrastructure.Inbox;

internal sealed class InboxMessageConsumerConfiguration : IEntityTypeConfiguration<InboxMessageConsumer>
{
    public void Configure(EntityTypeBuilder<InboxMessageConsumer> builder)
    {
        builder.ToTable("inbox_message_consumers");

        builder.HasKey(inboxMessageConsumer => new { inboxMessageConsumer.InboxMessageId, inboxMessageConsumer.Name });

        builder.Property(inboxMessageConsumer => inboxMessageConsumer.Name).HasMaxLength(500);
    }
}