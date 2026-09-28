using LocalAI.Application.Models;

namespace LocalAI.Application.Abstractions;

public interface IChatService
{
    Task<ChatResponse> SendMessageAsync(
        ChatRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> StreamMessageAsync(
        ChatRequest request,
        CancellationToken cancellationToken = default);
}