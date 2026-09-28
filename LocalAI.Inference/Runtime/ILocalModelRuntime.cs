using LocalAI.Application.Models;

namespace LocalAI.Inference.Runtime;

public interface ILocalModelRuntime
{
    Task LoadModelAsync(
        string modelPath,
        int contextSize,
        CancellationToken cancellationToken = default);

    Task<string> GenerateAsync(
        IReadOnlyList<ChatMessageRequest> messages,
        float temperature,
        int maxTokens,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> GenerateStreamingAsync(
        IReadOnlyList<ChatMessageRequest> messages,
        float temperature,
        int maxTokens,
        CancellationToken cancellationToken = default);

    Task UnloadModelAsync(
        CancellationToken cancellationToken = default);

    bool IsLoaded { get; }

    string? LoadedModelPath { get; }
}