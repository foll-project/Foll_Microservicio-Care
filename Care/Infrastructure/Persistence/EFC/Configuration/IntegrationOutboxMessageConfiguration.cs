using foll_care.Care.Infrastructure.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace foll_care.Care.Infrastructure.Persistence.EFC.Configuration;

public class IntegrationOutboxMessageConfiguration : IEntityTypeConfiguration<IntegrationOutboxMessage>
{
    public void Configure(EntityTypeBuilder<IntegrationOutboxMessage> builder)
    {
        builder.ToTable("integration_outbox_messages", "care");
        builder.HasKey(m => m.EventId);
        builder.Property(m => m.EventId).ValueGeneratedNever();
        builder.Property(m => m.EventType).HasMaxLength(150).IsRequired();
        builder.Property(m => m.Payload).HasColumnType("jsonb").IsRequired();
        builder.Property(m => m.LastError).HasMaxLength(2000);
        builder.HasIndex(m => m.OperationId);
        builder.HasIndex(m => m.NextAttemptAtUtc).HasFilter("published_at_utc IS NULL");
    }
}
