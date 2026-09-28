using LocalAI.Application.Models;

namespace LocalAI.Application.Abstractions;

public interface IInferenceEngine
{
    Task<InferenceResponse> GenerateAsync(
        InferenceRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> GenerateStreamingAsync(
        InferenceRequest request,
        CancellationToken cancellationToken = default);
}