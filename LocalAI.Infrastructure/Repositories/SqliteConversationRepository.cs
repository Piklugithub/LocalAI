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

        if (record is null)
        {
            return null;
        }

        record.Messages = record.Messages
            .OrderBy(x => x.CreatedAt)
            .ToList();

        return MapToDomain(record);
    }

    public async Task SaveAsync(
    Conversation conversation,
    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conversation);

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

            var domainMessageIds = conversation.Messages
                .Select(x => x.Id)
                .ToHashSet();

            // Remove messages that no longer exist in the domain conversation.
            var messagesToRemove = existingConversation.Messages
                .Where(x => !domainMessageIds.Contains(x.Id))
                .ToList();

            foreach (var message in messagesToRemove)
            {
                dbContext.Messages.Remove(message);
            }

            // Add new messages and update existing messages.
            foreach (var message in conversation.Messages)
            {
                var existingMessage =
                    existingConversation.Messages
                        .FirstOrDefault(x => x.Id == message.Id);

                if (existingMessage is null)
                {
                    dbContext.Messages.Add(
                        new ChatMessageRecord
                        {
                            Id = message.Id,
                            ConversationId = message.ConversationId,
                            Role = message.Role,
                            Content = message.Content,
                            CreatedAt = message.CreatedAt
                        });
                }
                else
                {
                    existingMessage.Role = message.Role;
                    existingMessage.Content = message.Content;
                    existingMessage.CreatedAt = message.CreatedAt;
                }
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
            .ToListAsync(cancellationToken);

        return records
            .OrderByDescending(x => x.UpdatedAt)
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