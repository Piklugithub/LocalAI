using LocalAI.Application.Abstractions;
using LocalAI.Domain.Entities;

namespace LocalAI.Application.Services;

public sealed class ConversationService(
    IConversationRepository conversationRepository)
    : IConversationService
{
    public Task<IReadOnlyList<Conversation>> GetConversationsAsync(
        CancellationToken cancellationToken = default)
    {
        return conversationRepository.GetAllAsync(
            cancellationToken);
    }

    public Task<Conversation?> GetConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        return conversationRepository.GetByIdAsync(
            conversationId,
            cancellationToken);
    }
}