using LocalAI.Application.Abstractions;
using LocalAI.Domain.Entities;
using LocalAI.Domain.Enums;
using LocalAI.Infrastructure.Persistence;
using LocalAI.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace LocalAI.Infrastructure.Repositories;

public sealed class SqliteConversationRepository(
    LocalAIDbContext dbContext)
    : IConversationRepository
{
    public async Task<Conversation?> GetByIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var record = await dbContext.Conversations
            .AsNoTracking()
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(
                x => x.Id == conversationId,
                cancellationToken);

        return record is null
            ? null
            : MapToDomain(record);
    }

    public async Task SaveAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default)
    {
        var existingConversation =
            await dbContext.Conversations
                .Include(x => x.Messages)
                .FirstOrDefaultAsync(
                    x => x.Id == conversation.Id,
                    cancellationToken);

        if (existingConversation is null)
        {
            var record = MapToRecord(conversation);

            dbContext.Conversations.Add(record);
        }
        else
        {
            existingConversation.Title = conversation.Title;
            existingConversation.UpdatedAt = conversation.UpdatedAt;

            existingConversation.Messages.Clear();

            foreach (var message in conversation.Messages)
            {
                existingConversation.Messages.Add(
                    new ChatMessageRecord
                    {
                        Id = message.Id,
                        ConversationId = message.ConversationId,
                        Role = message.Role,
                        Content = message.Content,
                        CreatedAt = message.CreatedAt
                    });
            }
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<IReadOnlyList<Conversation>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var records = await dbContext.Conversations
            .AsNoTracking()
            .Include(x => x.Messages)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        return records
            .Select(MapToDomain)
            .ToList();
    }

    private static ConversationRecord MapToRecord(
        Conversation conversation)
    {
        return new ConversationRecord
        {
            Id = conversation.Id,
            Title = conversation.Title,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt,
            Messages = conversation.Messages
                .Select(message => new ChatMessageRecord
                {
                    Id = message.Id,
                    ConversationId = message.ConversationId,
                    Role = message.Role,
                    Content = message.Content,
                    CreatedAt = message.CreatedAt
                })
                .ToList()
        };
    }

    private static Conversation MapToDomain(
        ConversationRecord record)
    {
        var conversation = Conversation.Rehydrate(
            record.Id,
            record.Title,
            record.CreatedAt,
            record.UpdatedAt);

        foreach (var message in record.Messages
                     .OrderBy(x => x.CreatedAt))
        {
            conversation.AddMessage(
                ChatMessage.Rehydrate(
                    message.Id,
                    message.ConversationId,
                    message.Role,
                    message.Content,
                    message.CreatedAt));
        }

        return conversation;
    }
}