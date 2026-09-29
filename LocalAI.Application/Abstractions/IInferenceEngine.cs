using LocalAI.Application.Models;

namespace LocalAI.Application.Abstractions;

public interface IInferenceEngine
{
    Task LoadModelAsync(
    string modelPath,
    int contextSize,
    CancellationToken cancellationToken = default);

    Task<InferenceResponse> GenerateAsync(
        InferenceRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> GenerateStreamingAsync(
        InferenceRequest request,
        CancellationToken cancellationToken = default);
}