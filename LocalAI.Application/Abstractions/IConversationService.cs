using LocalAI.Domain.Entities;

namespace LocalAI.Application.Abstractions;

public interface IConversationService
{
    Task<IReadOnlyList<Conversation>> GetConversationsAsync(
        CancellationToken cancellationToken = default);

    Task<Conversation?> GetConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);
}