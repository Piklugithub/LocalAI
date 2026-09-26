using LocalAI.Application.Models;

namespace LocalAI.Application.Abstractions;

public interface IChatService
{
    Task<ChatResponse> SendMessageAsync(
        ChatRequest request,
        CancellationToken cancellationToken = default);
}