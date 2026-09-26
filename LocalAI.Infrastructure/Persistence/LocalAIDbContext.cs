using LocalAI.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace LocalAI.Infrastructure.Persistence;

public sealed class LocalAIDbContext(
    DbContextOptions<LocalAIDbContext> options)
    : DbContext(options)
{
    public DbSet<ConversationRecord> Conversations =>
        Set<ConversationRecord>();

    public DbSet<ChatMessageRecord> Messages =>
        Set<ChatMessageRecord>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(LocalAIDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}