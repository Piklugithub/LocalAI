using LocalAI.Domain.Enums;

namespace LocalAI.Infrastructure.Persistence.Models;

public sealed class ChatMessageRecord
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public ChatRole Role { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public ConversationRecord? Conversation { get; set; }
}