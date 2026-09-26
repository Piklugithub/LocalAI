using LocalAI.Domain.Entities;

namespace LocalAI.Application.Abstractions;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Conversation>> GetAllAsync(
        CancellationToken cancellationToken = default);
}