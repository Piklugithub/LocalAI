using System.Diagnostics;
using LocalAI.Application.Abstractions;
using LocalAI.Application.Models;
using LocalAI.Inference.Runtime;

namespace LocalAI.Inference;

public sealed class LlamaInferenceEngine(
    ILocalModelRuntime runtime) : IInferenceEngine
{
    public async Task<InferenceResponse> GenerateAsync(
        InferenceRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.ModelPath))
        {
            throw new ArgumentException(
                "A model path is required.",
                nameof(request));
        }

        if (request.Messages.Count == 0)
        {
            throw new ArgumentException(
                "At least one message is required.",
                nameof(request));
        }

        var stopwatch = Stopwatch.StartNew();

        await runtime.LoadModelAsync(
            request.ModelPath,
            request.ContextSize,
            cancellationToken);

        var content = await runtime.GenerateAsync(
            request.Messages,
            request.Temperature,
            request.MaxTokens,
            cancellationToken);

        stopwatch.Stop();

        return new InferenceResponse
        {
            Content = content,
            InputTokens = null,
            OutputTokens = null,
            Duration = stopwatch.Elapsed
        };
    }
}