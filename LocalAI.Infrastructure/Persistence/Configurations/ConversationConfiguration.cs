using LocalAI.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalAI.Infrastructure.Persistence.Configurations;

public sealed class ConversationConfiguration
    : IEntityTypeConfiguration<ConversationRecord>
{
    public void Configure(
        EntityTypeBuilder<ConversationRecord> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasMany(x => x.Messages)
            .WithOne(x => x.Conversation)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}