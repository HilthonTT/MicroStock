using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Outbox;

internal sealed class OutboxMessageConsumerConfiguration : IEntityTypeConfiguration<OutboxMessageConsumer>
{
    public void Configure(EntityTypeBuilder<OutboxMessageConsumer> builder)
    {
        builder.ToTable("outbox_message_consumers");

        builder.HasKey(outboxMessageConsumer => new { outboxMessageConsumer.OutboxMessageId, outboxMessageConsumer.Name });

        builder.Property(outboxMessageConsumer => outboxMessageConsumer.Name).HasMaxLength(500);
    }
}