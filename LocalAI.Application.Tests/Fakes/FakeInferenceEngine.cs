using LocalAI.Application.Abstractions;
using LocalAI.Application.Models;

namespace LocalAI.Application.Tests.Fakes;

public sealed class FakeInferenceEngine : IInferenceEngine
{
    public Task<InferenceResponse> GenerateAsync(
        InferenceRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            new InferenceResponse
            {
                Content = "This is a test response.",
                Duration = TimeSpan.FromMilliseconds(100),
                InputTokens = 10,
                OutputTokens = 5
            });
    }

    public IAsyncEnumerable<string> GenerateStreamingAsync(
    InferenceRequest request,
    CancellationToken cancellationToken = default)
    {
        return StreamResponseAsync();
    }

    private static async IAsyncEnumerable<string> StreamResponseAsync()
    {
        yield return "Test ";
        yield return "response";

        await Task.CompletedTask;
    }
}