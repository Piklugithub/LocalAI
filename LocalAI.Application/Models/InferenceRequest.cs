using LocalAI.Domain.Enums;

namespace LocalAI.Application.Models;

public sealed class InferenceRequest
{
    public required string ModelId { get; init; }

    public required string ModelPath { get; init; }

    public required IReadOnlyList<ChatMessageRequest> Messages { get; init; }

    public float Temperature { get; init; } = 0.7f;

    public int MaxTokens { get; init; } = 1024;

    public int ContextSize { get; init; } = 8192;
}