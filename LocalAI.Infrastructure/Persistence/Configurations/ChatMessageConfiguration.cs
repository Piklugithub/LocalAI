using LocalAI.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalAI.Infrastructure.Persistence.Configurations;

public sealed class ChatMessageConfiguration
    : IEntityTypeConfiguration<ChatMessageRecord>
{
    public void Configure(
        EntityTypeBuilder<ChatMessageRecord> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Role)
            .IsRequired();

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.ConversationId);

        builder.HasIndex(x => x.CreatedAt);
    }
}