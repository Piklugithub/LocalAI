using LocalAI.Application.Abstractions;
using LocalAI.Domain.Entities;

namespace LocalAI.Application.Tests.Fakes;

public sealed class FakeConversationRepository : IConversationRepository
{
    private readonly List<Conversation> _conversations = [];

    public Task<Conversation?> GetByIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var conversation = _conversations
            .FirstOrDefault(x => x.Id == conversationId);

        return Task.FromResult(conversation);
    }

    public Task SaveAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default)
    {
        if (!_conversations.Contains(conversation))
        {
            _conversations.Add(conversation);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Conversation>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Conversation> result = _conversations;

        return Task.FromResult(result);
    }
}