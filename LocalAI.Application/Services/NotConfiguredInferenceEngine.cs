using LocalAI.Application.Abstractions;
using LocalAI.Application.Models;

namespace LocalAI.Application.Services;

public sealed class NotConfiguredInferenceEngine
    : IInferenceEngine
{
    public Task<InferenceResponse> GenerateAsync(
        InferenceRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException(
            "The local inference engine has not been configured yet.");
    }

    public IAsyncEnumerable<string> GenerateStreamingAsync(
    InferenceRequest request,
    CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException(
            "The local inference engine has not been configured yet.");
    }
}