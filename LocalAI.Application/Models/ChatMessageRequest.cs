using LocalAI.Domain.Enums;

namespace LocalAI.Application.Models;

public sealed class ChatMessageRequest
{
    public required ChatRole Role { get; init; }

    public required string Content { get; init; }
}