using LocalAI.Application.Models;

namespace LocalAI.Application.Abstractions;

public interface IInferenceEngine
{
    Task<InferenceResponse> GenerateAsync(
        InferenceRequest request,
        CancellationToken cancellationToken = default);
}