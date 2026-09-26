using LocalAI.Domain.Enums;

namespace LocalAI.Domain.Entities;

public class ChatMessage
{
    public Guid Id { get; private set; }

    public Guid ConversationId { get; private set; }

    public ChatRole Role { get; private set; }

    public string Content { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private ChatMessage()
    {
        Content = string.Empty;
    }

    public ChatMessage(
        Guid conversationId,
        ChatRole role,
        string content)
    {
        if (conversationId == Guid.Empty)
        {
            throw new ArgumentException(
                "Conversation ID cannot be empty.",
                nameof(conversationId));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException(
                "Message content cannot be empty.",
                nameof(content));
        }

        Id = Guid.NewGuid();
        ConversationId = conversationId;
        Role = role;
        Content = content;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static ChatMessage Rehydrate(
        Guid id,
        Guid conversationId,
        ChatRole role,
        string content,
        DateTimeOffset createdAt)
    {
        return new ChatMessage
        {
            Id = id,
            ConversationId = conversationId,
            Role = role,
            Content = content,
            CreatedAt = createdAt
        };
    }
}