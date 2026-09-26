namespace LocalAI.Infrastructure.Persistence.Models;

public sealed class ConversationRecord
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public List<ChatMessageRecord> Messages { get; set; } = [];
}